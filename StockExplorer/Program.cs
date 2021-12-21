using System;
using System.IO;
using Aspose.Cells;
using System.Collections.Generic;
using Npgsql;
using System.Security.Cryptography;
using System.Net.Http;
using Newtonsoft.Json;

namespace StockExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            string licfile = "AsposeLicense.txt";
            string apppath = Environment.CurrentDirectory;
            if (apppath.Substring(apppath.Length - 1, 1) != Path.DirectorySeparatorChar.ToString())
            {
                apppath = apppath + Path.DirectorySeparatorChar;
            }
            Console.WriteLine("Working Folder : " + apppath);

            string targetpath = "";
            string licpath = "";
            string temppath = "";
            string appconf = "convertconf.txt";
            bool err = false;

            if (System.IO.File.Exists(apppath + appconf) == false)
            {
                Console.WriteLine("Please create convertconf.txt at " + apppath + " with content TARGET_DIRECTORY|LICENCE_DIRECTORY|TEMP_DIRECTORY");
                err = true;
            }
            else
            {
                string apptext = System.IO.File.ReadAllText(apppath + appconf);
                apptext = apptext.Replace("\n", "");
                if (string.IsNullOrEmpty(apptext))
                {
                    err = true;
                    Console.WriteLine("convertconf.txt must have content TARGET_DIRECTORY|LICENCE_DIRECTORY|TEMP_DIRECTORY");
                }
                else
                {
                    string[] strconf = apptext.Split("|");
                    if (strconf[0].Trim() == "")
                    {
                        err = true;
                        Console.WriteLine("convertconf.txt must have content TARGET_DIRECTORY");
                    }
                    else
                    {
                        targetpath = strconf[0];
                        if (targetpath.Substring(targetpath.Length - 1, 1) != Path.DirectorySeparatorChar.ToString())
                        {
                            targetpath = targetpath + Path.DirectorySeparatorChar;
                        }
                        if (Directory.Exists(targetpath) == false)
                        {
                            err = true;
                            Console.WriteLine("Folder Target " + targetpath + " is not exist");
                        }
                        else
                        {
                            Console.WriteLine("Target Folder : " + targetpath);

                        }
                    }

                    if (err == false)
                    {
                        if (strconf.Length > 1)
                        {
                            if (strconf[1].Trim() == "")
                            {
                                licpath = apppath;
                                Console.WriteLine("License Folder is empty in " + appconf + " (using Working Folder : " + licpath + ")");
                            }
                            else
                            {
                                licpath = strconf[1];
                                if (licpath.Substring(licpath.Length - 1, 1) != Path.DirectorySeparatorChar.ToString())
                                {
                                    licpath = licpath + Path.DirectorySeparatorChar;
                                }
                                if (Directory.Exists(licpath) == false)
                                {
                                    err = true;
                                    Console.WriteLine("Folder License " + licpath + " is not exist");
                                }
                            }
                        }
                    }
                    if (err == false)
                    {
                        if (strconf.Length > 2)
                        {
                            if (strconf[2].Trim() == "")
                            {
                                temppath = apppath;
                                Console.WriteLine("Temporary Folder is empty in " + appconf + " (using Working Folder : " + licpath + ")");
                            }
                            else
                            {
                                temppath = strconf[2];
                                if (temppath.Substring(temppath.Length - 1, 1) != Path.DirectorySeparatorChar.ToString())
                                {
                                    temppath = temppath + Path.DirectorySeparatorChar;
                                }
                                if (Directory.Exists(temppath) == false)
                                {
                                    err = true;
                                    Console.WriteLine("Folder Temporary " + temppath + " is not exist");
                                }
                            }
                        }

                        if (err == false)
                        {
                            if (File.Exists(licpath + "AsposeLicense.txt") == false)
                            {
                                err = true;
                                Console.WriteLine("File AsposeLicense.txt is not exist in " + licpath);
                            }
                        }
                    }
                }//apptext==""
            }//check appconf
            if (err == false)
            {
                try
                {
                    // Create a License object
                    Aspose.Cells.License license = new License();

                    // Set the license of Aspose.Cells to avoid the evaluation limitations
                    // Uncomment this line if you have a license
                    //license.SetLicense(Get_SourceDirectory() + "Aspose.Cells.lic");
                    //license.SetLicense("/Users/tbamir/Downloads/nuget/AsposeLicense.txt");
                    license.SetLicense(licpath + "AsposeLicense.txt");
                }
                catch (Exception ex)
                {
                    err = true;
                    Console.WriteLine(ex.Message);
                }
            }

            if (err)
            {
                return;
            }

            string dbhost = "";
            string dbpass = "";
            int islocal = 0;
            if (islocal == 1)
            {
                dbhost = "localhost";
                dbpass = "Z3pp3l1n";
            }
            else
            {
                //dbhost = "10.34.41.11";
                dbhost = "10.34.29.59";
                dbpass = "kelekmambu^&*";
            }

            // ==================================
            //          EXPORT STOCK
            // ==================================

            string[] field_list;
            string[] header_list;

            field_list = new string[] { 
                "item_no","item_id", "task_id", "task_id_origin", "wh_id", 
                "site_id", "site_id_origin", "site_name_origin", "vendor", "division", 
                "project", "sourcenya", "kd_acs", "ne_id", 
                "flag_waste", "location", "pallet_id", "pnx_unit_category", "pnx_equipment_type", 
                "pnx_major_eqp", "wh_serial_num", "condition", "remark_condition", 
                "inbound_qty", "outbound_qty", "avl_qty", "pnx_manufacturing", 
                "pnx_eqp_category", "unit_of_measurement", "reuse_matrix", "status", 
                "inbound_date" 
            };
            header_list = new string[]{ 
                "No", "ITEM ID","Task Destination", "Task Origin", "warehouse ID", 
                "SITE ID DESTINATION", "Site ID Origin", "Site Name Origin", "Vendor", "Division", 
                "Project Name", "Ref Data", "code catalog", "NE ID", 
                "FLAG_WASTE", "Locator", "Pallet_ID","Part Number", "Description", 
                "Sub System", "Serial Number", "condition", "remark condition", 
                "inbound_qty", "outbound_qty", "avl_qty", "Brand", 
                "Equipment Type", "UoM", "Reuse Matrix", "Task Status", 
                "Inbound Date" 
            };
            var connection = new NpgsqlConnection("User ID=postgres;Password=" + dbpass + ";Host=" + dbhost + ";Database=pm_mobile_new;Port=5432;Timeout=300;CommandTimeout=300");
            connection.Open();

            Console.WriteLine("Start Stock Export " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //string fpath = "/Users/tbamir/Downloads/";

            int jmlrec = 0;
            Workbook wb = new Workbook();
            Worksheet sheet = wb.Worksheets[0];

            for (int i = 0; i < header_list.Length; i++)
            {
                int kolom = i;// + 1;
                Cell cell = sheet.Cells[0, kolom];

                //Style style = cell.GetStyle();
                //style.Custom = "#,##0.0000";
                //cell.SetStyle(style);
                cell.PutValue(header_list[i].ToUpper());
            }

            using (var command = connection.CreateCommand())
            {
                int baris = 1;

                string tbl = "mr_stock";
                string msql = "select * from " + tbl;
                command.CommandText = msql;

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int ix = 0; ix < field_list.Length; ix++)
                        {
                            int kolom = ix;// + 1;

                            Cell cell = sheet.Cells[baris, kolom];

                            string field = field_list[ix];
                            string fna = field;

                            if(fna=="item_no")
                            {
                                cell.PutValue(Convert.ToInt64(baris));
                                continue;
                            }

                            string ftype = reader.GetDataTypeName(reader.GetOrdinal(field));
                            string value = "";

                            if (ftype == "integer")
                            {
                                value = (reader.IsDBNull(reader.GetOrdinal(fna)) ? 0 : reader.GetInt64(reader.GetOrdinal(fna))).ToString();
                            }
                            else if (ftype.Contains("numeric"))
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetDouble(reader.GetOrdinal(fna)).ToString();
                            }
                            else if (ftype.Contains("timestamp"))
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetDateTime(reader.GetOrdinal(fna)).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetString(reader.GetOrdinal(fna));
                            }

                            if (field.Contains("qty"))
                            {
                                //Style style = cell.GetStyle();
                                //style.Custom = "#,##0.0000";
                                //cell.SetStyle(style);
                                cell.PutValue(Convert.ToInt64(value));
                            }
                            else
                            {
                                if (value != "")
                                {
                                    //Style style = cell.GetStyle();
                                    //style.Custom = "#,##0.0000";
                                    //cell.SetStyle(style);
                                    cell.PutValue(value.ToUpper().Trim());
                                }
                            }
                        }//for col

                        jmlrec = jmlrec + 1;
                        baris = baris + 1;
                    }//while reader.read
                }//using reader
                //                }//for(int iloop=0;iloop<2;iloop++)
            }//using command

            if (jmlrec > 0)
            {
                string pdate = DateTime.Now.ToString("yyyyMMdd");
                Random mnd = new Random();
                string export_filename_temp = "";
                int rnd = 0;
                bool found = true;
                while (found)
                {
                    rnd = mnd.Next(1000, 9999);
                    export_filename_temp = "STCK_";
                    export_filename_temp = pdate + "_" + export_filename_temp + "_temp" + rnd + ".xlsx";
                    if (System.IO.File.Exists(temppath + export_filename_temp) == false)
                    {
                        found = false;
                    }
                }
                Console.WriteLine("Save to Temporary " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("Temp File : " + temppath + export_filename_temp);
                wb.Save(temppath + export_filename_temp, Aspose.Cells.SaveFormat.Xlsx);
                Console.WriteLine("Save to Temporary Success " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string export_filename_export = "stock_" + pdate + ".xlsx";
                Console.WriteLine("Export File : " + temppath + export_filename_export);
                File.Copy(temppath + export_filename_temp, targetpath + export_filename_export, true);
                Console.WriteLine("Save to Export Success " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string targetpath_full = targetpath + export_filename_export;

                FileStream fs = new FileStream(targetpath_full, FileMode.Open, FileAccess.Read);
                modelAPIStatus aPIStatus = new modelAPIStatus();
                aPIStatus = UploadImage(targetpath_full, fs);

            }//jmlrec>0

            sheet.Dispose();
            wb.Dispose();
            Console.WriteLine("End Stock Export " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // ==================================
            //          EXPORT INBOUND
            // ==================================

            field_list = new string[] {
                "item_no","item_id", "task_id", "task_id_reference", "site_id",
                "site_id_origin", "vendor", "division", "project", "approval_0_user_name",
                "approval_1_user_name", "approval_2_user_name", "sourcenya", "kd_acs",
                "ne_id", "flag_waste", "locator_id", "pnx_unit_category", "pnx_equipment_type",
                "pnx_major_eqp", "wh_serial_num", "condition", "remark_condition",
                "qty_waste", "site_id", "pnx_manufacturing", "pnx_eqp_category",
                "unit_of_measurement", "reuse_matrix", "pnx_operation_status", "inbound_date",
                "task_remarks","pnx_mgr_remark","approval_remark","on_hand_qty",
                "in_transit_qty","allocated_qty","suspense_qty"
            };
            header_list = new string[] {
                "NO","ITEM ID", "TASK DESTINATION", "TASK ORIGIN", "SITE ID DESTINATION",
                "SITE ID ORIGIN", "VENDOR", "DIVISION", "PROJECT NAME", "APPROVAL 0 (VENDOR)",
                "APPROVAL 1 (PM INDOSAT)", "APPROVAL 2 (AMG)", "REF DATA", "CODE CATALOG",
                "NE ID", "FLAG_WASTE", "LOCATOR","PART NUMBER", "DESCRIPTION",
                "SUB SYSTEM", "SERIAL NUMBER", "CONDITION", "REMARK CONDITION",
                "QTY", "SITE ID DESTINATION", "BRAND", "EQUIPMENT TYPE",
                "UOM", "REUSE MATRIX", "MATERIAL STATUS",
                "INBOUND DATE","REMARK TASK","REMARK ITEM","REMARK APPROVAL",
                "ON HAND QTY","IN TRANSIT QTY","ALLOCATED QTY","SUSPENSE QTY"
            };

            Console.WriteLine("Start Inbound Export " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //string fpath = "/Users/tbamir/Downloads/";

            jmlrec = 0;
            wb = new Workbook();
            sheet = wb.Worksheets[0];

            for (int i = 0; i < header_list.Length; i++)
            {
                int kolom = i;// + 1;
                Cell cell = sheet.Cells[0, kolom];

                //Style style = cell.GetStyle();
                //style.Custom = "#,##0.0000";
                //cell.SetStyle(style);
                cell.PutValue(header_list[i].ToUpper());
            }

            using (var command = connection.CreateCommand())
            {
                int baris = 1;

                string tbl = "mr_inbound_transaction";
                string msql = "select * from " + tbl;
                command.CommandText = msql;

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < field_list.Length; i++)
                        {

                            int kolom = i;// + 1;

                            string field = field_list[i];
                            string fna = field;

                            Cell cell = sheet.Cells[baris, kolom];
                            if (fna == "item_no")
                            {
                                cell.PutValue(Convert.ToInt64(baris));
                                continue;
                            }

                            string ftype = reader.GetDataTypeName(reader.GetOrdinal(field));
                            string value = "";
                            if (ftype == "integer")
                            {
                                value = (reader.IsDBNull(reader.GetOrdinal(fna)) ? 0 : reader.GetInt64(reader.GetOrdinal(fna))).ToString();
                            }
                            else if (ftype.Contains("numeric"))
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetDouble(reader.GetOrdinal(fna)).ToString();
                            }
                            else if (ftype.Contains("timestamp"))
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetDateTime(reader.GetOrdinal(fna)).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetString(reader.GetOrdinal(fna));
                            }

                            if (field.Contains("qty"))
                            {
                                //Style style = cell.GetStyle();
                                //style.Custom = "#,##0.0000";
                                //cell.SetStyle(style);

                                try
                                {
                                    cell.PutValue(Convert.ToInt64(value));
                                }
                                catch
                                {
                                    try
                                    {
                                        cell.PutValue(Convert.ToDouble(value));
                                    }
                                    catch
                                    {
                                        cell.PutValue(Convert.ToString(value));
                                    }
                                }
                                
                            }
                            else
                            {
                                if (value != "")
                                {
                                    //Style style = cell.GetStyle();
                                    //style.Custom = "#,##0.0000";
                                    //cell.SetStyle(style);
                                    cell.PutValue(value.ToUpper().Trim());
                                }
                            }
                        }//for col

                        jmlrec = jmlrec + 1;
                        baris = baris + 1;
                    }//while reader.read
                }//using reader
                //                }//for(int iloop=0;iloop<2;iloop++)
            }//using command

            if (jmlrec > 0)
            {
                string pdate = DateTime.Now.ToString("yyyyMMdd");
                Random mnd = new Random();
                string export_filename_temp = "";
                int rnd = 0;
                bool found = true;
                while (found)
                {
                    rnd = mnd.Next(1000, 9999);
                    export_filename_temp = "INBD_";
                    export_filename_temp = pdate + "_" + export_filename_temp + "_temp" + rnd + ".xlsx";
                    if (System.IO.File.Exists(temppath + export_filename_temp) == false)
                    {
                        found = false;
                    }
                }
                Console.WriteLine("Save to Temporary " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("Temp File : " + temppath + export_filename_temp);
                wb.Save(temppath + export_filename_temp, Aspose.Cells.SaveFormat.Xlsx);
                Console.WriteLine("Save to Temporary Success " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string export_filename_export = "inbound_" + pdate + ".xlsx";
                Console.WriteLine("Export File : " + temppath + export_filename_export);
                File.Copy(temppath + export_filename_temp, targetpath + export_filename_export, true);
                Console.WriteLine("Save to Export Success " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string targetpath_full = targetpath + export_filename_export;

                FileStream fs = new FileStream(targetpath_full, FileMode.Open, FileAccess.Read);
                modelAPIStatus aPIStatus = new modelAPIStatus();
                aPIStatus = UploadImage(targetpath_full, fs);
            }//jmlrec>0

            sheet.Dispose();
            wb.Dispose();
            Console.WriteLine("End Inbound Export " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            // ==================================
            //          EXPORT OUTBOUND
            // ==================================

            field_list = new string[] {
                "item_no","mr_no", "mr_status", "item_id",
                "task_id", "task_id_destination", "site_id", "site_id_destination", "vendor",
                "division", "project", "approval_0_user_name", "approval_1_user_name",
                "approval_2_user_name", "sourcenya", "kd_acs", "ne_id", "flag_waste",
                "locator_id", "pnx_unit_category", "pnx_equipment_type", "pnx_major_eqp",
                "wh_serial_num", "condition", "remark_condition", "qty_waste",
                "pnx_manufacturing", "pnx_eqp_category", "unit_of_measurement", "reuse_matrix",
                "status","pnx_operation_status","inbound_date","in_transit_date",
                "task_remarks","pnx_mgr_remark","approval_remark"
            };
            header_list = new string[] {
                "NO", "MR NUMBER", "MR STATUS", "ITEM ID",
                "TASK ORIGIN", "TASK DESTINATION", "SITE ID ORIGIN", "SITE ID DESTINATION", "VENDOR",
                "DIVISION", "PROJECT NAME", "APPROVAL 0 (VENDOR)", "APPROVAL 1 (PM INDOSAT)",
                "APPROVAL 2 (AMG)", "REF DATA", "CODE CATALOG","NE ID", "FLAG WASTE",
                "LOCATOR", "PART NUMBER", "DESCRIPTION", "SUB SYSTEM",
                "SERIAL NUMBER", "CONDITION", "REMARK CONDITION", "QTY",
                "BRAND", "EQUIPMENT TYPE", "UOM", "REUSE MATRIX",
                "TASK STATUS","MATERIAL STATUS","INBOUND DATE","IN TRANSIT DATE",
                "REMARK TASK","REMARK ITEM","REMARK APPROVAL",
            };

            Console.WriteLine("Start Outbound Export " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            //string fpath = "/Users/tbamir/Downloads/";

            jmlrec = 0;
            wb = new Workbook();
            sheet = wb.Worksheets[0];

            for (int i = 0; i < header_list.Length; i++)
            {
                int kolom = i;// + 1;
                Cell cell = sheet.Cells[0, kolom];

                //Style style = cell.GetStyle();
                //style.Custom = "#,##0.0000";
                //cell.SetStyle(style);
                cell.PutValue(header_list[i].ToUpper());
            }

            using (var command = connection.CreateCommand())
            {
                int baris = 1;

                string tbl = "mr_outbound_transaction";
                string msql = "select * from " + tbl;
                command.CommandText = msql;

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        for (int i = 0; i < field_list.Length; i++)
                        {

                            int kolom = i;// + 1;

                            string field = field_list[i];
                            string fna = field;

                            Cell cell = sheet.Cells[baris, kolom];
                            if (fna == "item_no")
                            {
                                cell.PutValue(Convert.ToInt64(baris));
                                continue;
                            }

                            string ftype = reader.GetDataTypeName(reader.GetOrdinal(field));
                            string value = "";
                            if (ftype == "integer")
                            {
                                value = (reader.IsDBNull(reader.GetOrdinal(fna)) ? 0 : reader.GetInt64(reader.GetOrdinal(fna))).ToString();
                            }
                            else if (ftype.Contains("numeric"))
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetDouble(reader.GetOrdinal(fna)).ToString();
                            }
                            else if (ftype.Contains("timestamp"))
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetDateTime(reader.GetOrdinal(fna)).ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                value = reader.IsDBNull(reader.GetOrdinal(fna)) ? "" : reader.GetString(reader.GetOrdinal(fna));
                            }

                            if (field.Contains("qty"))
                            {
                                //Style style = cell.GetStyle();
                                //style.Custom = "#,##0.0000";
                                //cell.SetStyle(style);
                                try
                                {
                                    cell.PutValue(Convert.ToInt64(value));
                                }
                                catch
                                {
                                    try
                                    {
                                        cell.PutValue(Convert.ToDouble(value));
                                    }
                                    catch
                                    {
                                        cell.PutValue(Convert.ToString(value));
                                    }
                                }
                            }
                            else
                            {
                                if (value != "")
                                {
                                    //Style style = cell.GetStyle();
                                    //style.Custom = "#,##0.0000";
                                    //cell.SetStyle(style);
                                    cell.PutValue(value.ToUpper().Trim());
                                }
                            }
                        }//for col

                        jmlrec = jmlrec + 1;
                        baris = baris + 1;
                    }//while reader.read
                }//using reader
                //                }//for(int iloop=0;iloop<2;iloop++)
            }//using command

            if (jmlrec > 0)
            {
                string pdate = DateTime.Now.ToString("yyyyMMdd");
                Random mnd = new Random();
                string export_filename_temp = "";
                int rnd = 0;
                bool found = true;
                while (found)
                {
                    rnd = mnd.Next(1000, 9999);
                    export_filename_temp = "OUBD_";
                    export_filename_temp = pdate + "_" + export_filename_temp + "_temp" + rnd + ".xlsx";
                    if (System.IO.File.Exists(temppath + export_filename_temp) == false)
                    {
                        found = false;
                    }
                }
                Console.WriteLine("Save to Temporary " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                Console.WriteLine("Temp File : " + temppath + export_filename_temp);
                wb.Save(temppath + export_filename_temp, Aspose.Cells.SaveFormat.Xlsx);
                Console.WriteLine("Save to Temporary Success " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string export_filename_export = "outbound_" + pdate + ".xlsx";
                Console.WriteLine("Export File : " + temppath + export_filename_export);
                File.Copy(temppath + export_filename_temp, targetpath + export_filename_export, true);
                Console.WriteLine("Save to Export Success " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                string targetpath_full = targetpath + export_filename_export;

                FileStream fs = new FileStream(targetpath_full, FileMode.Open, FileAccess.Read);
                modelAPIStatus aPIStatus = new modelAPIStatus();
                aPIStatus = UploadImage(targetpath_full, fs);
            }//jmlrec>0

            sheet.Dispose();
            wb.Dispose();
            Console.WriteLine("End Outbound Export " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

            connection.Close();
        }//static Main

        public static modelAPIStatus UploadImage(string filePath, System.IO.Stream stream)
        {
            var url = "http://10.34.41.11/stock_export/php/";
            var atoolpage = "receive";

            var client = new System.Net.Http.HttpClient();

            //client.BaseAddress = new Uri(App.rootUrl + "php/");
            client.Timeout = new TimeSpan(0, 10, 0);

            //StreamContent scontent = new StreamContent(file.GetStream());
            StreamContent scontent = new StreamContent(stream);
            scontent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            scontent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                FileName = filePath,// "newimage",
                Name = "file[]"
            };

            var multi = new MultipartFormDataContent();
            multi.Add(scontent);

            StringContent studentIdContent = new StringContent("stock");
            multi.Add(studentIdContent, "datatype");

            //var uri = new Uri(string.Format(App.url + "php/index.php?act=ftag&user_id=" + App.userdata.user_id, string.Empty));
            var uri = new Uri(string.Format(url + atoolpage+".php?act=ftag", string.Empty));
            var response = client.PostAsync(uri, multi).Result;//.Result;
            //var response = await client.PostAsync(new Uri(App.baseUrl + "common1.php?" + strQuery), str);

            var placesJson = response.Content.ReadAsStringAsync().Result;
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
    }

    public class modelAPIStatus
    {
        [JsonProperty("CODE")]
        public string code { get; set; }
        [JsonProperty("MESSAGE")]
        public string reason { get; set; }
    }
}
