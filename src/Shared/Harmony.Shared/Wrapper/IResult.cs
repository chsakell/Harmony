namespace Harmony.Shared.Wrapper
{
    public interface IResult
    {
        List<string> Messages { get; set; }

        bool Succeeded { get; set; }
        ResultCode Code { get; set; }
    }

    public interface IResult<out T> : IResult
    {
        T Data { get; }
    }
}