using Google.Protobuf;
using Grpc.Core;
using Harmony.Api.IntegrationTests.Harness;
using Harmony.Api.Protos;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Automations and Notifications call the six gRPC services without a login, so they stay explicitly anonymous until
    /// R-09 (FR-105). One read method per service is called with no credentials and new random IDs. Any outcome but a
    /// refusal for missing authentication passes; grpc-dotnet reports an HTTP 401 as <see cref="StatusCode.Unauthenticated"/>.
    /// The calls use hand-built <see cref="Method{TRequest, TResponse}"/>s over the Api's own message types (plan,
    /// <i>Decisions</i>), so the test project generates no stubs.
    /// </summary>
    public class GrpcAnonymousTests(ApiTestHost host)
    {
        public static TheoryData<string> Services => new(MatrixCheck.GrpcServices.Order(StringComparer.Ordinal));

        [Theory]
        [Trait("Requirement", "FR-105")]
        [MemberData(nameof(Services))]
        public async Task GrpcService_AnonymousCall_IsNotUnauthenticated(string service)
        {
            var api = await host.GetAsync();
            using var channel = api.CreateGrpcChannel();
            var invoker = channel.CreateCallInvoker();
            var id = Guid.NewGuid().ToString();

            var call = service switch
            {
                "BoardService" => Call(invoker, service, "GetBoard", new BoardFilterRequest { BoardId = id }, BoardResponse.Parser),
                "CardService" => Call(invoker, service, "GetCard", new CardFilterRequest { CardId = id }, CardResponse.Parser),
                "UserCardService" => Call(invoker, service, "GetUserCard", new UserCardFilterRequest { CardId = id, UserId = id }, UserCardResponse.Parser),
                "WorkspaceService" => Call(invoker, service, "GetWorkspace", new WorkspaceFilterRequest { WorkspaceId = id }, WorkspaceResponse.Parser),
                "UserService" => Call(invoker, service, "GetUser", new UserFilterRequest { UserId = id }, UserResponse.Parser),
                "UserNotificationService" => Call(invoker, service, "GetUsersForNotificationType",
                    new GetUsersForNotificationTypeRequest { Users = { id }, Type = 0 }, GetUsersForNotificationTypeResponse.Parser),
                _ => throw new ArgumentOutOfRangeException(nameof(service), service, "Add a read method for this gRPC service."),
            };

            StatusCode status;
            try
            {
                await call;
                status = StatusCode.OK;
            }
            catch (RpcException exception)
            {
                status = exception.StatusCode;
            }

            status.ShouldNotBe(StatusCode.Unauthenticated, $"{service} must stay reachable without a login until R-09 (FR-105)");
        }

        private static async Task Call<TRequest, TResponse>(CallInvoker invoker, string service, string method, TRequest request,
            MessageParser<TResponse> parser)
            where TRequest : class, IMessage<TRequest>
            where TResponse : class, IMessage<TResponse>
        {
            var descriptor = new Method<TRequest, TResponse>(MethodType.Unary, service, method,
                Marshallers.Create<TRequest>(message => message.ToByteArray(), _ => throw new NotSupportedException()),
                Marshallers.Create(message => message.ToByteArray(), parser.ParseFrom));

            using var call = invoker.AsyncUnaryCall(descriptor, host: null, new CallOptions(cancellationToken: TestContext.Current.CancellationToken), request);
            await call.ResponseAsync;
        }
    }
}
