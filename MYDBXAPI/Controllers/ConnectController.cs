using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api;
using MYDBXAPI.Models;
using System.IO;
using Dropbox.Api.Files;
using Microsoft.AspNetCore.Cors;
using Microsoft.Extensions.Configuration;


namespace MYDBXAPI.Controllers
{

    [Route("api/[controller]/{action}")]
    [ApiController]
    [EnableCors("Access-Control-Allow-Origin")]
    public class ConnectController : ControllerBase
    {
        private readonly IConfiguration aConfig;
        public ConnectController(IConfiguration c) {
            aConfig = c;
        }

        DropboxClient dbx;



        public async Task<ApiUserModel> GetUserInfo(string usertoken)
        {
            dbx = new DropboxClient(usertoken);
            var full = await dbx.Users.GetCurrentAccountAsync();
            
            return new ApiUserModel(full.Name.DisplayName, full.Email,full.ProfilePhotoUrl);

        }
        public async Task<List<FileModel>> GetFileList(string usertoken)
        {
            dbx = new DropboxClient(usertoken);
            List<FileModel> fList = new List<FileModel>();
            var list = await dbx.Files.ListFolderAsync(string.Empty);
            List<int> numbers = new List<int>();
            List<ListFolderResult> prevLists = new List<ListFolderResult>();
            for (int i = 0; i < list.Entries.Count; i++)
            {
                if (list.Entries[i].IsFile)
                {
                    fList.Add(new FileModel(list.Entries[i].Name, getSizeFormated(list.Entries[i].AsFile.Size.ToString()), list.Entries[i].PathLower));
                }
                else if (list.Entries[i].IsFolder)
                {
                    var innerList = await dbx.Files.ListFolderAsync(list.Entries[i].PathLower);

                    for (int j = 0; j < innerList.Entries.Count; j++)
                    {
                        if (innerList.Entries[j].IsFile)
                        {
                            fList.Add(new FileModel(innerList.Entries[j].Name, getSizeFormated(innerList.Entries[j].AsFile.Size.ToString()), innerList.Entries[j].PathLower));
                        }
                        else if (innerList.Entries[j].IsFolder)
                        {
                            prevLists.Add(innerList);
                            innerList = await dbx.Files.ListFolderAsync(innerList.Entries[j].PathLower);
                            numbers.Add(j + 1);
                            j = -1;
                        }

                        if ((j == innerList.Entries.Count - 1 && numbers.Count > 0) || innerList.Entries.Count == 0)
                        {
                            int last = numbers[numbers.Count - 1];
                            innerList = prevLists[prevLists.Count - 1];
                            j = last - 1;
                            numbers.RemoveAt(numbers.Count - 1);
                            prevLists.RemoveAt(prevLists.Count - 1);
                        }

                    }

                }
            }




            return fList;


        }
        public async Task<byte[]> DownloadFile(string usertoken = "", string filepath = "")
        {

            dbx = new DropboxClient(usertoken);
            using (var response = await dbx.Files.DownloadAsync(filepath))
            {
                var x = await response.GetContentAsByteArrayAsync();

                return x;
            }
        }
        [HttpPost]
        public async Task UploadFile(string usertoken, FileModel f)
        {
            var filepath = f.filepath;
            var filename = f.filename;
            if (filepath is null || filepath=="Root") { 
                filepath = "/" + f.filename; }
            else
            {
                filepath += "/" + filename;
            }
            dbx = new DropboxClient(usertoken);
            var fileAsBytes = f.filebytes;


            using (var mem = new MemoryStream(fileAsBytes))
            {
                var updated = await dbx.Files.UploadAsync(
                    filepath,
                    WriteMode.Overwrite.Instance,
                    body: mem);

            }
        }

        public async Task<string> Auth(string code, string redirectUrll)
        {
            //Dropbox api'in yönlendirdiği task
            try
            {
                var response = await DropboxOAuth2Helper.ProcessCodeFlowAsync(
                    code,
                    aConfig.GetSection("DropboxApp").GetSection("AppKey").Value,
                    aConfig.GetSection("DropboxApp").GetSection("AppSecret").Value,
                    redirectUrll);
                var dropboxAccessToken = response.AccessToken;

                //diğer fonksiyonlara sürekli parametre olarak göndermemek için tokeni şimdilik globale taşıdım


                return dropboxAccessToken;

            }
            catch (Exception e)
            {
                return "errortoken";

            }

        }




        [HttpGet]
        public ActionResult Get()
        {
            // test
            return Redirect("https://www.google.com");
        }

        [HttpGet]
        public string GetLoginLink(string redirectUrll)
        {
            // başlangıç isteği
            // login linkini dönüyor
            var state = Guid.NewGuid().ToString("N");
            var redirect = DropboxOAuth2Helper.GetAuthorizeUri(
                OAuthResponseType.Code,
                 aConfig.GetSection("DropboxApp").GetSection("AppKey").Value,
                redirectUrll,
                state);

            return redirect.ToString();
        }


        public async Task<string> DeleteItem(string usertoken = "", string filepath = "")
        {
            if (filepath != "" && filepath != null && filepath != "/")
            {
                dbx = new DropboxClient(usertoken);
                var response = await dbx.Files.DeleteV2Async(filepath);
                return "completed";
            }
            else {
                return "wrong path or path is root ";
            }
        }

        public async Task<string> CreateFolder(string usertoken = "", string filepath = "")
        {
            dbx = new DropboxClient(usertoken);
            var response = await dbx.Files.CreateFolderV2Async(filepath);
            return "completed";
        }


        public async Task<List<string>> GetFolderPathList(string usertoken)
        {
            // sadece klasörleri getirir
            dbx = new DropboxClient(usertoken);
            List<string> folders = new List<string>();
            var list = await dbx.Files.ListFolderAsync(string.Empty);
            List<int> numbers = new List<int>();
            List<ListFolderResult> prevLists = new List<ListFolderResult>();
            for (int i = 0; i < list.Entries.Count; i++)
            {
                if (list.Entries[i].IsFile)
                {
                    
                }
                else if (list.Entries[i].IsFolder)
                {
                    if (!folders.Contains(list.Entries[i].PathLower))
                    {
                        folders.Add(list.Entries[i].PathLower);
                    }

                    var innerList = await dbx.Files.ListFolderAsync(list.Entries[i].PathLower);
                    

                    for (int j = 0; j < innerList.Entries.Count; j++)
                    {
                        if (innerList.Entries[j].IsFile)
                        {
                            
                        }
                        else if (innerList.Entries[j].IsFolder)
                        {
                            if (!folders.Contains(innerList.Entries[j].PathLower))
                            {
                                folders.Add(innerList.Entries[j].PathLower);
                            }
                            prevLists.Add(innerList);
                            innerList = await dbx.Files.ListFolderAsync(innerList.Entries[j].PathLower);
                            
                            numbers.Add(j + 1);
                            j = -1;
                        }

                        if ((j == innerList.Entries.Count - 1 && numbers.Count > 0) || innerList.Entries.Count == 0)
                        {
                            int last = numbers[numbers.Count - 1];
                            innerList = prevLists[prevLists.Count - 1];
                            j = last - 1;
                            numbers.RemoveAt(numbers.Count - 1);
                            prevLists.RemoveAt(prevLists.Count - 1);
                        }

                    }

                }
            }
            return folders;
        }

        public string getSizeFormated(string filesize)
        {
            int size = Int32.Parse(filesize);
            int len = filesize.Length;

            if (size < 1024)
            {
                filesize = filesize + " B";
                return filesize;
            }
            else if (size > 1024 && size < 1048576)
            {
                size /= 1024;
                return size.ToString() + " KB";
            }
            else if (size > 1048576)
            {
                size /= 1048576;
                return size.ToString() + " MB";
            }

            return "sizeerror";
        }

        
    }
}
