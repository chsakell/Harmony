namespace Harmony.Notifications.EmailTemplates
{
    public partial class EmailTemplates
    {
        public static string BuildFromNoActionGenericTemplate(
            string host, string title, string firstName,
            string emailNotification, string customerAction)
        {
            return NoActionGenericTemplate
                .Replace("{host}", host)
                .Replace("{hello-first-name}", $"Hello {firstName}")
                .Replace("{title}", title)
                .Replace("{message-1}", emailNotification)
                .Replace("{message-2}", customerAction);
        }

        public const string NoActionGenericTemplate = @"<!DOCTYPE html>

<html lang=""en"" xmlns:o=""urn:schemas-microsoft-com:office:office"" xmlns:v=""urn:schemas-microsoft-com:vml"">
<head>
<title></title>
<meta content=""text/html; charset=utf-8"" http-equiv=""Content-Type""/>
<meta content=""width=device-width, initial-scale=1.0"" name=""viewport""/><!--[if mso]><xml><o:OfficeDocumentSettings><o:PixelsPerInch>96</o:PixelsPerInch><o:AllowPNG/></o:OfficeDocumentSettings></xml><![endif]--><!--[if !mso]><!-->
<link href=""https://fonts.googleapis.com/css?family=Montserrat"" rel=""stylesheet"" type=""text/css""/><!--<![endif]-->
<style>
		* {
			box-sizing: border-box;
		}

		body {
			margin: 0;
			padding: 0;
		}

		a[x-apple-data-detectors] {
			color: inherit !important;
			text-decoration: inherit !important;
		}

		#MessageViewBody a {
			color: inherit;
			text-decoration: none;
		}

		p {
			line-height: inherit
		}

		.desktop_hide,
		.desktop_hide table {
			mso-hide: all;
			display: none;
			max-height: 0px;
			overflow: hidden;
		}

		.image_block img+div {
			display: none;
		}

		@media (max-width:620px) {

			.desktop_hide table.icons-inner,
			.social_block.desktop_hide .social-table {
				display: inline-block !important;
			}

			.icons-inner {
				text-align: center;
			}

			.icons-inner td {
				margin: 0 auto;
			}

			.image_block div.fullWidth {
				max-width: 100% !important;
			}

			.mobile_hide {
				display: none;
			}

			.row-content {
				width: 100% !important;
			}

			.stack .column {
				width: 100%;
				display: block;
			}

			.mobile_hide {
				min-height: 0;
				max-height: 0;
				max-width: 0;
				overflow: hidden;
				font-size: 0px;
			}

			.desktop_hide,
			.desktop_hide table {
				display: table !important;
				max-height: none !important;
			}
		}
	</style>
</head>
<body style=""background-color: #833e5a; margin: 0; padding: 0; -webkit-text-size-adjust: none; text-size-adjust: none;"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""nl-container"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #833e5a;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; width: 600px; margin: 0 auto;"" width=""600"">
<tbody>
<tr>
<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""image_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""pad"" style=""padding-top:25px;width:100%;padding-right:0px;padding-left:0px;"">
<div align=""center"" class=""alignment"" style=""line-height:10px"">
<div class=""fullWidth"" style=""max-width: 600px;""><img alt=""Image"" src=""{host}/images/rounder-up.png"" style=""display: block; height: auto; border: 0; width: 100%;"" title=""Image"" width=""600""/></div>
</div>
</td>
</tr>
</table>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-2"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #FFFFFF; color: #000000; width: 600px; margin: 0 auto;"" width=""600"">
<tbody>
<tr>
<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""image_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""pad"" style=""padding-top:25px;width:100%;padding-right:0px;padding-left:0px;"">
<div align=""center"" class=""alignment"" style=""line-height:10px"">
<div style=""max-width: 120px;""><img alt=""Harmony"" src=""{host}/images/harmony-logo.jpg"" style=""display: block; height: auto; border: 0; width: 100%;"" title=""Harmony"" width=""120""/></div>
</div>
</td>
</tr>
</table>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-3"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #FFFFFF; color: #000000; width: 600px; margin: 0 auto;"" width=""600"">
<tbody>
<tr>
<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
<table border=""0"" cellpadding=""10"" cellspacing=""0"" class=""paragraph_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
<tr>
<td class=""pad"">
<div style=""color:#0D0D0D;font-family:'Montserrat', 'Trebuchet MS', 'Lucida Grande', 'Lucida Sans Unicode', 'Lucida Sans', Tahoma, sans-serif;font-size:28px;line-height:120%;text-align:center;mso-line-height-alt:33.6px;"">
<p style=""margin: 0; word-break: break-word;""><span><strong><span>{hello-first-name},</span></strong></span><br/><span>{title}</span></p>
</div>
</td>
</tr>
</table>
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""image_block block-2"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""pad"" style=""width:100%;padding-right:0px;padding-left:0px;"">
<div align=""center"" class=""alignment"" style=""line-height:10px"">
<div style=""max-width: 316px;""><img alt=""Image"" src=""{host}/images/divider.png"" style=""display: block; height: auto; border: 0; width: 100%;"" title=""Image"" width=""316""/></div>
</div>
</td>
</tr>
</table>
<table border=""0"" cellpadding=""10"" cellspacing=""0"" class=""text_block block-3"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
<tr>
<td class=""pad"">
<div style=""font-family: sans-serif"">
<div class="""" style=""font-size: 12px; font-family: 'Montserrat', 'Trebuchet MS', 'Lucida Grande', 'Lucida Sans Unicode', 'Lucida Sans', Tahoma, sans-serif; mso-line-height-alt: 18px; color: #555555; line-height: 1.5;"">
<p style=""margin: 0; font-size: 14px; text-align: center; mso-line-height-alt: 21px;"">{message-1}<span style=""color:#a8bf6f;font-size:14px;""><strong><br/></strong></span></p>
</div>
</div>
</td>
</tr>
</table>
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""image_block block-4"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""pad"" style=""width:100%;padding-right:0px;padding-left:0px;"">
<div align=""center"" class=""alignment"" style=""line-height:10px"">
<div style=""max-width: 330px;""><img alt=""Image"" src=""{host}/images/evening-sunshine.jpg"" style=""display: block; height: auto; border: 0; width: 100%;"" title=""Image"" width=""330""/></div>
</div>
</td>
</tr>
</table>
<table border=""0"" cellpadding=""10"" cellspacing=""0"" class=""text_block block-3"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
	<tr>
	<td class=""pad"">
	<div style=""font-family: sans-serif"">
	<div class="""" style=""font-size: 12px; font-family: 'Montserrat', 'Trebuchet MS', 'Lucida Grande', 'Lucida Sans Unicode', 'Lucida Sans', Tahoma, sans-serif; mso-line-height-alt: 18px; color: #555555; line-height: 1.5;"">
	<p style=""margin: 0; font-size: 14px; text-align: center; mso-line-height-alt: 21px;"">{message-2}<span style=""color:#a8bf6f;font-size:14px;""><strong><br/></strong></span></p>
	</div>
	</div>
	</td>
	</tr>
	</table>
<div class=""spacer_block block-7"" style=""height:40px;line-height:40px;font-size:1px;""> </div>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-4"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; background-color: #2360b1; width: 600px; margin: 0 auto;"" width=""600"">
<tbody>
<tr>
<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""33.333333333333336%"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""social_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""pad"" style=""padding-top:15px;text-align:center;padding-right:0px;padding-left:0px;"">
<div align=""center"" class=""alignment"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""social-table"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; display: inline-block;"" width=""74px"">
<tr>
<td style=""padding:0 5px 0 0px;""><a href=""{facebook-link}"" target=""_blank""><img alt=""Facebook"" height=""32"" src=""{host}/images/facebook2x.png"" style=""display: block; height: auto; border: 0;"" title=""Facebook"" width=""32""/></a></td>
<td style=""padding:0 5px 0 0px;""><a href=""{twitter-link}"" target=""_blank""><img alt=""Twitter"" height=""32"" src=""{host}/images/twitter2x.png"" style=""display: block; height: auto; border: 0;"" title=""Twitter"" width=""32""/></a></td>
</tr>
</table>
</div>
</td>
</tr>
</table>
</td>
<td class=""column column-2"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""33.333333333333336%"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""paragraph_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
<tr>
<td class=""pad"" style=""padding-top:20px;"">
<div style=""color:#b62c2a;font-family:'Montserrat', 'Trebuchet MS', 'Lucida Grande', 'Lucida Sans Unicode', 'Lucida Sans', Tahoma, sans-serif;font-size:12px;font-weight:700;line-height:120%;text-align:center;mso-line-height-alt:14.399999999999999px;""> </div>
</td>
</tr>
</table>
</td>
<td class=""column column-3"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""33.333333333333336%"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""text_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; word-break: break-word;"" width=""100%"">
<tr>
<td class=""pad"" style=""padding-top:20px;"">
<div style=""font-family: sans-serif"">
<div class="""" style=""font-size: 12px; font-family: 'Montserrat', 'Trebuchet MS', 'Lucida Grande', 'Lucida Sans Unicode', 'Lucida Sans', Tahoma, sans-serif; mso-line-height-alt: 14.399999999999999px; color: #b62c2a; line-height: 1.2;"">
<p style=""margin: 0; font-size: 12px; mso-line-height-alt: 14.399999999999999px;""> </p>
</div>
</div>
</td>
</tr>
</table>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-5"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; width: 600px; margin: 0 auto;"" width=""600"">
<tbody>
<tr>
<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
<div class=""spacer_block block-1"" style=""height:60px;line-height:60px;font-size:1px;""> </div>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row row-6"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; background-color: #ffffff;"" width=""100%"">
<tbody>
<tr>
<td>
<table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class=""row-content stack"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; color: #000000; background-color: #ffffff; width: 600px; margin: 0 auto;"" width=""600"">
<tbody>
<tr>
<td class=""column column-1"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt; font-weight: 400; text-align: left; padding-bottom: 5px; padding-top: 5px; vertical-align: top; border-top: 0px; border-right: 0px; border-bottom: 0px; border-left: 0px;"" width=""100%"">
<table border=""0"" cellpadding=""0"" cellspacing=""0"" class=""icons_block block-1"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""pad"" style=""vertical-align: middle; color: #1e0e4b; font-family: 'Inter', sans-serif; font-size: 15px; padding-bottom: 5px; padding-top: 5px; text-align: center;"">
<table cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""mso-table-lspace: 0pt; mso-table-rspace: 0pt;"" width=""100%"">
<tr>
<td class=""alignment"" style=""vertical-align: middle; text-align: center;""><!--[if vml]><table align=""center"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""display:inline-block;padding-left:0px;padding-right:0px;mso-table-lspace: 0pt;mso-table-rspace: 0pt;""><![endif]-->
<!--[if !vml]><!-->
</td>
</tr>
</table>
</td>
</tr>
</table>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table>
</td>
</tr>
</tbody>
</table><!-- End -->
</body>
</html>";
    }
}
