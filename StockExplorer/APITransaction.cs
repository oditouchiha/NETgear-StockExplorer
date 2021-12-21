using System;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace amgclient
{
    public static class APITransaction
    {
        static string siteinfopage = "site_infoV5";//"site_infoV3";
        static string atoolpage = "atool_taskV5";//"atool_taskV3";

        static string grsiteinfopage = "atool_taskV4";//"site_infoV3";
        static string gratoolpage = "atool_taskV4";//"atool_taskV3";
        static string atoolpage_nody = "atool_taskv3 - nody";
        public static async Task<modelAPIStatus> UploadImage(String filetype, string filePath, System.IO.Stream stream)
        {
            var client = new System.Net.Http.HttpClient();

            //client.BaseAddress = new Uri(App.rootUrl + "php/");
            client.Timeout = new TimeSpan(0, 10, 0);

            //StreamContent scontent = new StreamContent(file.GetStream());
            StreamContent scontent = new StreamContent(stream);
            scontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                FileName = filePath,// "newimage",
                Name = "file[]"
            };
            scontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var multi = new MultipartFormDataContent();
            multi.Add(scontent);

            StringContent studentIdContent = new StringContent(filetype);
            multi.Add(studentIdContent, "file_type");

            //var uri = new Uri(string.Format(App.url + "php/index.php?act=ftag&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(App.url + atoolpage+".php?act=ftag&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.PostAsync(uri, multi);//.Result;
            //var response = await client.PostAsync(new Uri(App.baseUrl + "common1.php?" + strQuery), str);

            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var placesJson = await response.Content.ReadAsStringAsync();
                //var placesJson = response.Content.ReadAsStringAsync().Result;
                modelAPIStatus transaction_data = new modelAPIStatus();
                if (placesJson != "")
                {
                    try
                    {
                        //modelUpload mu = JsonConvert.DeserializeObject<modelUpload>(placesJson);
                        transaction_data = JsonConvert.DeserializeObject<modelAPIStatus>(placesJson);
                        //return transaction_data;
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        transaction_data= null;
                    }
                }
                return transaction_data;
            }
            else
            {
                return null;
            }

        }

        public static async Task<List<modelSite>> GetSite(string datatype = "", double longitude = 0, double latitude = 0,string site_id="")
        {
            HttpClient client = new HttpClient();
            //var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=SITE_INFO&user_id=" + App.userdata.user_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&DATATYPE=" + datatype + "&longitude=" + longitude + "&latitude=" + latitude + "&site_id=" + site_id, string.Empty));
            Uri uri;
            if (App.userdata.user_app == "gr")
            {
                uri = new Uri(string.Format(App.url + grsiteinfopage + ".php?act=grsitevw&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&site_id=" + site_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            else
            {
                uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=SITE_INFO&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&DATATYPE=" + datatype + "&longitude=" + longitude + "&latitude=" + latitude + "&site_id=" + site_id, string.Empty));
            }
            var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                //System.Collections.Generic.List<modelSite> sites = new System.Collections.Generic.List<modelSite>();
                try
                {
                    site_view cat_data = new site_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<site_view>(content);
                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }

                /*App.site_list.Clear();
                foreach (modelSite site in sites)
                {
                    site.last = true;


                    /*string slat = site.lat;
                    slat = slat.Replace(",", ".");
                    double lat = Convert.ToDouble(slat);
                    string slong = site.pnx_lat_long;
                    slong = slong.Replace(",", ".");
                    double longx = Convert.ToDouble(slong);*/
                /*
                                    site.longitude = site.LONG;
                                    site.latitude = site.LAT;

                                    var request = new GeolocationRequest(GeolocationAccuracy.Medium);
                                    var location = await Geolocation.GetLocationAsync(request);
                                    Location sourceCoordinates = new Location(location.Latitude, location.Longitude);
                                    Location destinationCoordinates = new Location(site.latitude, site.longitude);
                                    double distance = Location.CalculateDistance(sourceCoordinates, destinationCoordinates, DistanceUnits.Kilometers);

                                    if (distance < 1)
                                    {
                                        site.site_type = "nearby";
                                    }

                                    App.site_list.Add(site);
                                    List<modelSite> stx = await App.Database.GetSite(site.site_id);
                                    if (stx.Count == 0)
                                    {
                                        App.Database.SaveSite(site);
                                    }
                                }*/
            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTask>> GetTaskBySiteID(string site_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_TASK&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&site_id=" + site_id, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelAcs>> GetAcs(string category = "", int maxacs = 0)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=gacs&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("category=" + category + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&last_version=" + maxacs.ToString(), Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.PostAsync(uri, str);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        acs_view cat_data = new acs_view();
                        if (content != "")
                        {
                            cat_data = JsonConvert.DeserializeObject<acs_view>(content);
                            App.APIStatus.code = cat_data.code;
                            App.APIStatus.reason = cat_data.reason;
                            App.APIStatus.success = true;
                        }
                        return cat_data.modelCat;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public static async Task<List<modelItemMR>> GetMR(string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=gmr&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild , Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.PostAsync(uri, str);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        mr_view cat_data = new mr_view();
                        if (content != "")
                        {
                            cat_data = JsonConvert.DeserializeObject<mr_view>(content);
                            App.APIStatus.code = cat_data.code;
                            App.APIStatus.reason = cat_data.reason;
                            App.APIStatus.success = true;
                        }
                        return cat_data.modelCat;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public static async Task<List<modelBuffer>> GetBuffer(string site_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=gbf&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("site_id_lsp=" + site_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.PostAsync(uri, str);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        buffer_view cat_data = new buffer_view();
                        if (content != "")
                        {
                            cat_data = JsonConvert.DeserializeObject<buffer_view>(content);
                            App.APIStatus.code = cat_data.code;
                            App.APIStatus.reason = cat_data.reason;
                            App.APIStatus.success = true;
                        }
                        return cat_data.modelCat;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }


        }
        public static async Task<modelAPIStatus> CekLogin()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=cklg", string.Empty));
            StringContent str = new StringContent("user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild , Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> CreateClaim(string site_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=atskclaim&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("site_id=" + site_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> CreateTask(string site_id, string ne_id, string vendor)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage+".php?act=atsk&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("site_id=" + site_id + "&ne_id=" + ne_id + "&os=" +  Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&vendor=" + vendor, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> CreateGRTask(string po_id, string line_id, string seq)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + gratoolpage + ".php?act=grdownload&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("po_id=" + po_id + "&line_id=" + line_id + "&seq=" + seq + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild , Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> CreateTaskSite(string site_id, string ne_code, string ne_id, string ticket_id, string work_type, string site_id_lsp, string task_type)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=atsksite&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("site_id=" + site_id + "&ne_code=" + ne_code + "&ne_id=" + ne_id + "&ticket_id=" + ticket_id + "&site_id_lsp=" + site_id_lsp + "&task_type=" + task_type + "&work_type=" + work_type + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> CreateTaskNonPC(string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage+".php?act=atskatp&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> SiteCheck(string site_id, string ne_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_SITE_PRIV&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("site_id=" + site_id+ "&ne_id=" + ne_id, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> UpdateTask(string task_id, string zipfilename)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage+".php?act=utsk&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&zipfilename=" + zipfilename, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> UpdateTaskV2(string task_id, string zipfilename)
        {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 5, 0);
            var uri = new Uri(string.Format(App.url + atoolpage+".php?act=utsk3&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&zipfilename=" + zipfilename, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> UpdateTaskClaim(string task_id, string zipfilename)
        {
            HttpClient client = new HttpClient();
            client.Timeout = new TimeSpan(0, 5, 0);
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=utskc&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&zipfilename=" + zipfilename, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> UpdateTaskGR(string task_id, string po_id, string zipfilename, string remark, string qty_in)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + gratoolpage + ".php?act=utsk&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&po_id=" + po_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&remark=" + remark + "&qty_in=" + qty_in + "&zipfilename=" + zipfilename, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTask>> GetTaskCompleteSummary()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=TASK_COMPLETE_SUMMARY&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelATPReportData>> GetATPTaskStatusSummary(string task_type,string region="",string area="",string cluster="")
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_ATP_STATUS_SUMMARY&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token + "&task_type=" + task_type + "&region=" + region + "&area=" + area + "&cluster=" + cluster, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    atp_summary_view cat_data = new atp_summary_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<atp_summary_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelATPReportData>> GetATPTaskDateSummary(string task_type, string region, string period,string area="",string cluster="")
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+"x.php?hit=GET_ATP_STATUS_PERIOD&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token + "&task_type=" + task_type + "&region=" + region + "&area=" + area + "&cluster=" + cluster + "&period=" + period, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    atp_summary_view cat_data = new atp_summary_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<atp_summary_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTask>> GetTaskStatusSummary(string task_type)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_TASK_STATUS_SUMMARY&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token + "&task_type=" + task_type, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTask>> GetTaskMonthSummary(string task_type)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_TASK_MONTH_SUMMARY&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token + "&task_type=" + task_type, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelInsuranceTask>> GetClaimByTaskID(string task_id)
        {
            HttpClient client = new HttpClient();
            Uri uri;
                uri = new Uri(string.Format(App.url + atoolpage + ".php?act=gtskclaim&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    insurance_task_view cat_data = new insurance_task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<insurance_task_view>(content);

                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTask>> GetTaskByTaskID(string task_id, string work_type="")
        {
            if (string.IsNullOrEmpty(work_type)) work_type = "";
            HttpClient client = new HttpClient();
            Uri uri ;
            if(App.userdata.user_app=="gr")
            {
                uri = new Uri(string.Format(App.url + grsiteinfopage + ".php?act=grtaskvw&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id + "&work_type=" + work_type + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            else
            {
                uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_TASK&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);

                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTask>> GetTaskByType(string task_type, int start = 0, int limit = 10, string task_id = "", string site_id = "", string ne_id = "", string reference_task_id = "", string status = "", string work_type = "")
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Connection", "close");
            client.Timeout = TimeSpan.FromMinutes(5);

            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_TASK&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_type=" + task_type + "&start=" + start + "&limit=" + limit + "&xtask_id=" + task_id + "&xsite_id=" + site_id + "&xne_id=" + ne_id + "&xreference_task_id=" + reference_task_id + "&xstatus=" + status + "&work_type=" + work_type + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }//gettaskbytype
        public static async Task<List<modelTask>> GetTaskPATP(string task_id)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Connection", "close");
            client.Timeout = TimeSpan.FromMinutes(5);
            var uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_PATP_DETAIL&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id  + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }//gettaskbytype
        public static async Task<List<modelTask>> GetTaskByMe(string task_type, int start = 0, int limit = 10, string task_id = "", string site_id = "", string ne_id = "", string reference_task_id = "", string status = "", string work_type = "",string count_only="")
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Connection", "close");
            client.Timeout = TimeSpan.FromMinutes(5);
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_TASK_BY_ME&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_type=" + task_type + "&start=" + start + "&limit=" + limit + "&xtask_id=" + task_id + "&xsite_id=" + site_id + "&xne_id=" + ne_id + "&xreference_task_id=" + reference_task_id + "&xstatus=" + status + "&work_type=" + work_type + "&countonly=" + count_only + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            if (count_only != "1" && string.IsNullOrEmpty(task_id) == false)
            {
                int x = 1;
            }
            if (count_only != "1" && string.IsNullOrEmpty(work_type) == false)
            {
                int x = 1;
            }
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (count_only == "")
                {
                    int x = 1;
                }
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }//gettaskbytype
        public static async Task<List<modelTask>> GetGRTaskByType(string task_type, int start = 0, int limit = 10, string task_id = "", string site_id = "", string ne_id = "", string reference_task_id = "", string status = "", string work_type = "", string count_only = "")
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Connection", "close");
            client.Timeout = TimeSpan.FromMinutes(5);

            var uri = new Uri(string.Format(App.url + gratoolpage + ".php?act=grtaskvw&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_type=" + task_type + "&start=" + start + "&limit=" + limit + "&xtask_id=" + task_id + "&xsite_id=" + site_id + "&xne_id=" + ne_id + "&xreference_task_id=" + reference_task_id + "&xstatus=" + status + "&work_type=" + work_type + "&count_only=" + count_only + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }//gettaskbytype
        public static async Task<List<modelTask>> GetGRTaskByMe(string task_type, int start = 0, int limit = 10, string task_id = "", string site_id = "", string ne_id = "", string reference_task_id = "", string status = "", string work_type = "", string count_only = "")
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Add("Connection", "close");
            client.Timeout = TimeSpan.FromMinutes(5);
            var uri = new Uri(string.Format(App.url + grsiteinfopage + ".php?act=grtaskvw&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_type=" + task_type + "&start=" + start + "&limit=" + limit + "&xtask_id=" + task_id + "&xsite_id=" + site_id + "&xne_id=" + ne_id + "&xreference_task_id=" + reference_task_id + "&xstatus=" + status + "&work_type=" + work_type + "&countonly=" + count_only + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                if (count_only == "")
                {
                    int x = 4;
                }
                if (string.IsNullOrEmpty(task_id) == false)
                {
                    int x = 4;
                }
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }//gettaskbytype
        public static async Task<List<modelTask>> GetTaskStatus(string task_type)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_TASK_STATUS&user_id=" + App.userdata.user_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild + "&token=" + App.userdata.token + "&task_type=" + task_type, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    task_view cat_data = new task_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<task_view>(content);
                    }
                    return cat_data.modelCat;
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelNE>> GetNE(string site_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_NE_ID&user_id=" + App.userdata.user_id + "&site_id=" + site_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    ne_view cat_data = new ne_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<ne_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelSite>> GetLSP()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage + ".php?act=glsp&user_id=" + App.userdata.user_id +  "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    site_view cat_data = new site_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<site_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelItem>> GetItemBySN(string site_id, string serial_no)
        {
            HttpClient client = new HttpClient();
            Uri uri;
                uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_EQUIPMENT_INFO_SN&user_id=" + App.userdata.user_id + "&site_id=" + site_id + "&serial_no=" + serial_no + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            App.APIStatus.success = false;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    item_view cat_data = new item_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<item_view>(content);
                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelItem>> GetClaimItem(string task_id)
        {
            HttpClient client = new HttpClient();
            Uri uri;
            uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_CLAIM_ITEM_INFO&user_id=" + App.userdata.user_id + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            App.APIStatus.success = false;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    item_view cat_data = new item_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<item_view>(content);
                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelPATPPhoto>> GetPATPItem(string task_id )
        {
            HttpClient client = new HttpClient();
            Uri uri;
            uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_ACTIVITY_INFO&user_id=" + App.userdata.user_id + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            App.APIStatus.success = false;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    patpphoto_view cat_data = new patpphoto_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<patpphoto_view>(content);
                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelItem>> GetItem(string ne_id, string task_id = "", string work_type = "", string download = "", string line_id_list = "", string po_id = "")
        {
            HttpClient client = new HttpClient();
            Uri uri;
            if(App.userdata.user_app=="gr")
            {
                uri = new Uri(string.Format(App.url + grsiteinfopage + ".php?act=gritemvw&user_id=" + App.userdata.user_id + "&ne_id=" + ne_id + "&task_id=" + task_id + "&work_type=" + work_type + "&download=" + download + "&po_id=" + po_id + "&line_id_list=" + line_id_list + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            else
            {
                uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_EQUIPMENT_INFO&user_id=" + App.userdata.user_id + "&ne_id=" + ne_id + "&task_id=" + task_id + "&work_type=" + work_type + "&download=" + download + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            App.APIStatus.code = "";
            App.APIStatus.reason = "";
            App.APIStatus.success = false;
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    item_view cat_data = new item_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<item_view>(content);
                        modelAPIStatus cat_datax = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_datax.code;
                        App.APIStatus.reason = cat_datax.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelItem>> GetItemTracking(string task_id )
        {
            HttpClient client = new HttpClient();
            Uri uri;
            if(App.userdata.user_app=="gr")
            {
                uri = new Uri(string.Format(App.url + gratoolpage + ".php?act=gritemdownloadvw&user_id=" + App.userdata.user_id + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            else
            {
                uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_EQUIPMENT_TRACKING_INFO&user_id=" + App.userdata.user_id + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    item_view cat_data = new item_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<item_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTaskHistory>> GetHistory(string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_HISTORY&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            if (App.userdata.user_app == "gr")
            {
                uri = new Uri(string.Format(App.url + grsiteinfopage + ".php?act=grhisvw&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            }
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    history_view cat_data = new history_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<history_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTaskPriviledge>> GetPrivilege(string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage+".php?hit=GET_PRIVILEDGE&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    privilege_view cat_data = new privilege_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<privilege_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelTblQuestion>> GetQuestion()
        {
            HttpClient client = new HttpClient();
            //var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gquestion&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gquestion&user_id=B41278076" + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            //StringContent str = new StringContent("&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.GetAsync(uri);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        question_view question_data = new question_view();
                        if (content != "")
                        {
                            question_data = JsonConvert.DeserializeObject<question_view>(content);
                            App.APIStatus.code = question_data.code;
                            App.APIStatus.reason = question_data.reason;
                            App.APIStatus.success = true;
                        }
                        return question_data.modelTblQuestion;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static async Task<List<modelTblQuestionOption>> GetQuestionOption()
        {
            HttpClient client = new HttpClient();
            //var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gquestionoption&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gquestionoption&user_id=B41278076" + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            //StringContent str = new StringContent("&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.GetAsync(uri);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        question_option_view question_option_data = new question_option_view();
                        if (content != "")
                        {
                            question_option_data = JsonConvert.DeserializeObject<question_option_view>(content);
                            App.APIStatus.code = question_option_data.code;
                            App.APIStatus.reason = question_option_data.reason;
                            App.APIStatus.success = true;
                        }
                        return question_option_data.modelTblQuestionOption;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static async Task<List<modelRTaskAnswer>> GetAnswer()
        {
            HttpClient client = new HttpClient();
            //var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gquestion&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=ganswer&user_id=B41278076" + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            //StringContent str = new StringContent("&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.GetAsync(uri);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        answer_view answer_data = new answer_view();
                        if (content != "")
                        {
                            answer_data = JsonConvert.DeserializeObject<answer_view>(content);
                            App.APIStatus.code = answer_data.code;
                            App.APIStatus.reason = answer_data.reason;
                            App.APIStatus.success = true;
                        }
                        return answer_data.modelRTaskAnswer;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static async Task<List<modelRTaskPhoto>> GetPhoto()
        {
            HttpClient client = new HttpClient();
            //var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gphoto&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=gphoto&user_id="+App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            //StringContent str = new StringContent("&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            try
            {
                var response = await client.GetAsync(uri);
                App.APIStatus.success = false;
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    try
                    {
                        photo_view photo_data = new photo_view();
                        if (content != "")
                        {
                            photo_data = JsonConvert.DeserializeObject<photo_view>(content);
                            App.APIStatus.code = photo_data.code;
                            App.APIStatus.reason = photo_data.reason;
                            App.APIStatus.success = true;
                        }
                        return photo_data.modelRTaskPhoto;
                        //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        return null;
                    }


                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> SubmitAnswer(int q_no, string q_answer_description, string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=sans&user_id="+App.userdata.user_id, string.Empty));
            StringContent str = new StringContent("q_no=" + q_no + "&q_answer_description=" + q_answer_description + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> GetReport(string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + "generate_pdf_check.php?task_id=" + task_id, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> SubmitTask(string task_id, string site_id, string user_id, double lat, double longi)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=stask&user_id="+App.userdata.user_id, string.Empty));
            StringContent str = new StringContent("task_id=" + task_id + "&site_id=" + site_id + "&user_id=" + App.userdata.user_id + "&lat=" + lat + "&longi=" + longi + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> SubmitPhoto(int q_no, string picture_attachment, string task_id)
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=spho&user_id="+ App.userdata.user_id, string.Empty));
            StringContent str = new StringContent("q_no=" + q_no + "&picture_attachment=" + picture_attachment + "&task_id=" + task_id + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, Encoding.UTF8, "application/x-www-form-urlencoded");
            var response = await client.PostAsync(uri, str);
            //var response = await client.GetAsync(uri);
            App.APIStatus.success = false;
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    modelAPIStatus cat_data = new modelAPIStatus();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<modelAPIStatus>(content);
                        App.APIStatus.code = cat_data.code;
                        App.APIStatus.reason = cat_data.reason;
                        App.APIStatus.success = true;
                    }
                    return cat_data;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<modelAPIStatus> UploadPhoto(string filePath, System.IO.Stream stream)
        {
            var client = new System.Net.Http.HttpClient();

            //client.BaseAddress = new Uri(App.rootUrl + "php/");
            client.Timeout = new TimeSpan(0, 10, 0);

            //StreamContent scontent = new StreamContent(file.GetStream());
            StreamContent scontent = new StreamContent(stream);
            scontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                FileName = filePath,// "newimage",
                Name = "file[]"
            };
            scontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");

            var multi = new MultipartFormDataContent();
            multi.Add(scontent);

            //StringContent studentIdContent = new StringContent(filetype);
            //multi.Add(studentIdContent, "file_type");

            //var uri = new Uri(string.Format(App.url + "php/index.php?act=ftag&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(App.url + atoolpage_nody + ".php?act=upho&user_id="+ App.userdata.user_id + "&token=" + App.userdata.token + " & os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.PostAsync(uri, multi);//.Result;
                                                              //var response = await client.PostAsync(new Uri(App.baseUrl + "common1.php?" + strQuery), str);

            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var placesJson = await response.Content.ReadAsStringAsync();
                //var placesJson = response.Content.ReadAsStringAsync().Result;
                modelAPIStatus transaction_data = new modelAPIStatus();
                if (placesJson != "")
                {
                    try
                    {
                        //modelUpload mu = JsonConvert.DeserializeObject<modelUpload>(placesJson);
                        transaction_data = JsonConvert.DeserializeObject<modelAPIStatus>(placesJson);
                        //return transaction_data;
                    }
                    catch (Newtonsoft.Json.JsonSerializationException ex)
                    {
                        transaction_data = null;
                    }
                }
                return transaction_data;
            }
            else
            {
                return null;
            }

        }

        public static async Task<List<modelAcsITMajorEquipment>> GetAcsITMajor()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=ACS_IT_MAJOR_EQUIPMENT&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    acs_it_major_view cat_data = new acs_it_major_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<acs_it_major_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
        public static async Task<List<modelInsuranceClaim>> GetClaimType()
        {
            HttpClient client = new HttpClient();
            var uri = new Uri(string.Format(App.url + siteinfopage + ".php?hit=GET_CLAIM&user_id=" + App.userdata.user_id + "&token=" + App.userdata.token + "&os=" + Device.RuntimePlatform + "&vrs=" + VersionTracking.CurrentBuild, string.Empty));
            var response = await client.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                try
                {
                    claim_view cat_data = new claim_view();
                    if (content != "")
                    {
                        cat_data = JsonConvert.DeserializeObject<claim_view>(content);
                    }
                    return cat_data.modelCat;
                    //sites = cat_data.modelCat;// JsonConvert.DeserializeObject<System.Collections.Generic.List<modelSite>>(content);
                }
                catch (Newtonsoft.Json.JsonSerializationException ex)
                {
                    return null;
                }


            }
            else
            {
                return null;
            }

        }
    }

    public class claim_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        public string code { get; set; }
        [JsonProperty("DATA")]
        public List<modelInsuranceClaim> modelCat { get; set; }
    }
    public class acs_it_major_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        public string code { get; set; }
        [JsonProperty("DATA")]
        public List<modelAcsITMajorEquipment> modelCat { get; set; }
    }
    public class site_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        public string code { get; set; }
        [JsonProperty("DATA")]
        public List<modelSite> modelCat { get; set; }
    }
    public class buffer_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelBuffer> modelCat { get; set; }
    }
    public class task_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        public string code { get; set; }
        [JsonProperty("DATA")]
        public List<modelTask> modelCat { get; set; }
    }
    public class insurance_task_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        public string code { get; set; }
        [JsonProperty("DATA")]
        public List<modelInsuranceTask> modelCat { get; set; }
    }
    public class single_task_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public modelTask modelCat { get; set; }
    }
    public class ne_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        [JsonProperty("DATA")]
        public List<modelNE> modelCat { get; set; }
    }
    public class item_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        [JsonProperty("DATA")]
        public List<modelItem> modelCat { get; set; }
    }
    public class patpphoto_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        [JsonProperty("DATA")]
        public List<modelPATPPhoto> modelCat { get; set; }
    }
    public class history_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        [JsonProperty("DATA")]
        public List<modelTaskHistory> modelCat { get; set; }
    }
    public class mr_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelItemMR> modelCat { get; set; }
    }
    public class privilege_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        [JsonProperty("DATA")]
        public List<modelTaskPriviledge> modelCat { get; set; }
    }
    public class user_view
    {
        [JsonProperty("CODE")]
        string CODE { get; set; }
        [JsonProperty("DATA")]
        public modelUser modelCat { get; set; }
    }
    public class acs_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelAcs> modelCat { get; set; }
    }
    public class atp_summary_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelATPReportData> modelCat { get; set; }
    }
    public class question_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelTblQuestion> modelTblQuestion { get; set; }
    }
    public class question_option_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelTblQuestionOption> modelTblQuestionOption { get; set; }
    }
    public class answer_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelRTaskAnswer> modelRTaskAnswer { get; set; }
    }
    public class photo_view
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
        [JsonProperty("DATA")]
        public List<modelRTaskPhoto> modelRTaskPhoto { get; set; }
    }
}
