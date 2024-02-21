using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Rebar;

namespace QRCode
{
    public partial class frm_CreateQR : Form
    {
        private Bank listBankData;
        private string URLBank = "https://api.vietqr.io/v2/banks";
        private string URLQRCode = "https://api.vietqr.io/v2/generate";
        public frm_CreateQR()
        {
            InitializeComponent();
            listBankData = new Bank();
            using (WebClient client = new WebClient())
            {
                var htmlData = client.DownloadData(URLBank);
                var bankRawJson = Encoding.UTF8.GetString(htmlData);
                listBankData = JsonConvert.DeserializeObject<Bank>(bankRawJson);

                cb_nganhang.DataSource = listBankData.data;   // list banks
                cb_nganhang.DisplayMember = "shortName";
                cb_nganhang.ValueMember = "bin";
                cb_template.SelectedIndex = 3;
            }

            txtSTK.Text = "77000039790101";
            txtTenTaiKhoan.Text = "Dam Van Luan";
            txtSoTien.Text = "6666666";
            txtNoiDung.Text = "Chuyen Khoan";
            setMaNH("970422");
        }
        void setMaNH(string maNH)
        {
            if (listBankData != null && listBankData.data != null && listBankData.data.Count > 0)
            {
                for (int i = 0; i < listBankData.data.Count; i++)
                {
                    if (maNH == listBankData.data[i].bin)
                    {

                        cb_nganhang.SelectedIndex = i;
                        return;
                    }
                }
            }
        }

        private void btn_CreateQRCode_Click(object sender, EventArgs e)
        {
            var apiRequest = new ApiRequest();
            apiRequest.acqId = Convert.ToInt32(cb_nganhang.SelectedValue.ToString());
            //MessageBox.Show(apiRequest.acqId.ToString());
            apiRequest.accountNo = long.Parse(txtSTK.Text);
            apiRequest.accountName = txtTenTaiKhoan.Text;
            apiRequest.amount = Convert.ToInt32(txtSoTien.Text);
            apiRequest.format = "text";
            apiRequest.template = cb_template.Text;
            apiRequest.addInfo = txtNoiDung.Text;
            var jsonRequest = JsonConvert.SerializeObject(apiRequest);
            // use restsharp for request api.
            //var client = new RestClient(URLQRCode);
            //var request = new RestRequest();

            //request.Method = Method.Post;
            //request.AddHeader("Accept", "application/json");

            //request.AddParameter("application/json", jsonRequest, ParameterType.RequestBody);

            //var response = client.Execute(request);
            //var content = response.Content;
            var content = PostAPI(URLQRCode, jsonRequest);
            var dataResult = JsonConvert.DeserializeObject<ApiResponse>(content);


            var image = Base64ToImage(dataResult.data.qrDataURL.Replace("data:image/png;base64,", ""));
            pictureBox1.Image = image;
        }

        public Image Base64ToImage(string base64String)
        {
            byte[] imageBytes = Convert.FromBase64String(base64String);
            MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length);
            ms.Write(imageBytes, 0, imageBytes.Length);
            System.Drawing.Image image = System.Drawing.Image.FromStream(ms, true);
            return image;
        }


        private string getAPI(string url)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.ContentType = "application/json";
            try
            {
                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();
                    return response;
                }
            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    //if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    //{

                    //}
                    return httpResponse.StatusCode.ToString();

                }
                return "error";
            }
        }

        private string PostAPI(string URL, string jsonRequest)
        {

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(URL);
            request.Method = "POST";
            request.ContentType = "application/json";
            byte[] postBytes = Encoding.UTF8.GetBytes(jsonRequest);
            request.ContentLength = postBytes.Length;
            Stream requestStream = request.GetRequestStream();

            // now send it
            requestStream.Write(postBytes, 0, postBytes.Length);
            requestStream.Close();
            try
            {
                WebResponse webResponse = request.GetResponse();
                using (Stream webStream = webResponse.GetResponseStream() ?? Stream.Null)
                using (StreamReader responseReader = new StreamReader(webStream))
                {
                    string response = responseReader.ReadToEnd();

                    MessageBox.Show(response);
                    return response;
                }

            }
            catch (WebException e)
            {
                using (WebResponse response = e.Response)
                {
                    HttpWebResponse httpResponse = (HttpWebResponse)response;
                    if (httpResponse.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        MessageBox.Show("Erro.");
                    }
                    return httpResponse.StatusCode.ToString();
                }
                return "error";
            }
        }
    }
}
