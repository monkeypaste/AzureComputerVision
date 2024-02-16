using MonkeyPaste.Common.Plugin;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace AzureComputerVision {
    public class AzureCvPlugin : MpIAnalyzeComponentAsync, MpISupportHeadlessAnalyzerFormat {
        const string PARAM_ID_VISUAL_FEATURES = "features";
        const string PARAM_ID_DETAILS = "details";
        const string PARAM_ID_CONTENT = "content";
        const string PARAM_ID_API_KEY = "apikey";
        const string PARAM_ID_API_REGION_URL = "region";
        const string PARAM_ID_SIGNUP = "signup";

        const string SIGNUP_URL = "https://azure.microsoft.com/en-us/free/";

        AzureCvAnnotator _annotator = new AzureCvAnnotator();
        public async Task<MpAnalyzerPluginResponseFormat> AnalyzeAsync(MpAnalyzerPluginRequestFormat req) {

            NameValueCollection query = HttpUtility.ParseQueryString(string.Empty);

            if (req.GetParamValue<List<string>>(PARAM_ID_VISUAL_FEATURES) is { } features &&
                features.Count > 0) {
                query.Add("visualFeatures", string.Join(",", features));
            }
            if (req.GetParamValue<List<string>>(PARAM_ID_DETAILS) is { } details &&
                details.Count > 0) {
                query.Add("details", string.Join(",", details));
            }

            if (query.Count == 0 ||
                Convert.FromBase64String(req.GetParamValue<string>(PARAM_ID_CONTENT)) is not byte[] imgBytes) {
                // no options specified 
                return null;
            }

            var resp = new MpAnalyzerPluginResponseFormat();
            if (req.GetParamValue<string>(PARAM_ID_API_REGION_URL) is not string region_url ||
                !Uri.IsWellFormedUriString(region_url, UriKind.Absolute)) {
                resp.invalidParams.Add(PARAM_ID_API_REGION_URL, Resources.RegionError);
                return resp;
            }
            string subscriptionKey = req.GetParamValue<string>(PARAM_ID_API_KEY);
            string endpoint_url = $"{region_url}vision/v3.2/analyze?{query}";
            using (var httpClient = new HttpClient()) {
                using (var request = new HttpRequestMessage(
                    new HttpMethod("POST"), endpoint_url)) {
                    request.Headers.TryAddWithoutValidation("Ocp-Apim-Subscription-Key", subscriptionKey);

                    request.Content = new ByteArrayContent(imgBytes);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                    try {
                        var http_response = await httpClient.SendAsync(request);
                        string http_response_str = await http_response.Content.ReadAsStringAsync();
                        if (http_response.IsSuccessStatusCode) {
                            resp = _annotator.CreateAnnotations(resp, http_response_str);
                        } else {
                            if ((int)http_response.StatusCode == 401) {
                                // invalidate creds
                                resp.invalidParams.Add(PARAM_ID_API_KEY, http_response_str);
                            } else {
                                resp.errorMessage = http_response_str;
                            }
                        }
                    }
                    catch (Exception ex) {
                        resp.errorMessage = ex.Message;
                    }

                }
            }
            return resp;
        }
        public MpAnalyzerComponent GetFormat(MpHeadlessComponentFormatRequest request) {
            Resources.Culture = new System.Globalization.CultureInfo(request.culture);

            return new MpAnalyzerComponent() {
                inputType = new MpPluginInputFormat() {
                    image = true
                },
                outputType = new MpPluginOutputFormat() {
                    imageAnnotation = true
                },
                parameters = new List<MpParameterFormat>() {
                    new MpParameterFormat() {
                        label = Resources.FeaturesLabel,
                        controlType = MpParameterControlType.MultiSelectList,
                        unitType = MpParameterValueUnitType.PlainText,
                        values =
                            new string[] {
                                Resources.Adult,
                                Resources.Brands,
                                Resources.Categories,
                                Resources.Color,
                                Resources.Descrip,
                                Resources.Faces,
                                Resources.Objects,
                                Resources.Taggs,
                            }
                            .Select((x,idx)=>new MpParameterValueFormat() {
                                label = x,
                                value = ((ComputerVisionFeatureType)idx).ToString().ToLowerInvariant(),
                                isDefault = true
                            }).ToList(),
                        paramId = PARAM_ID_VISUAL_FEATURES,
                    },
                    new MpParameterFormat() {
                        label = Resources.DetailsLabel,
                        controlType = MpParameterControlType.MultiSelectList,
                        unitType = MpParameterValueUnitType.PlainText,
                        values =
                            new string[] {
                                Resources.Celebrities,
                                Resources.Landmarks
                            }
                            .Select((x,idx)=>new MpParameterValueFormat() {
                                label = x,
                                value = ((ComputerDetailType)idx).ToString().ToLowerInvariant(),
                                isDefault = true
                            }).ToList(),
                        paramId = PARAM_ID_DETAILS,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        isSharedValue = true,
                        isRequired = true,
                        label = Resources.RegionLabel,
                        description = Resources.RegionDescription,
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = PARAM_ID_API_REGION_URL,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        isSharedValue = true,
                        isRequired = true,
                        label = Resources.KeyLabel,
                        description = Resources.KeyDescription,
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainText,
                        paramId = PARAM_ID_API_KEY,
                    },
                    new MpParameterFormat() {
                        isExecuteParameter = true,
                        controlType = MpParameterControlType.Hyperlink,
                        value = new MpParameterValueFormat(SIGNUP_URL,Resources.SignUpLabel,true),
                        paramId = PARAM_ID_SIGNUP,
                    },
                    new MpParameterFormat() {
                        isVisible = false,
                        isRequired = true,
                        label = "Source Content",
                        controlType = MpParameterControlType.TextBox,
                        unitType = MpParameterValueUnitType.PlainTextContentQuery,
                        value = new MpParameterValueFormat("{ClipText}",true),
                        paramId = PARAM_ID_CONTENT,
                    },
                }
            };
        }
    }
}
