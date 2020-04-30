using System;
using System.IO;
using System.Net;
using System.Linq;

namespace ViBot
{
    class Visit
    {
        private CookieCollection Cookies;

        public Visit(string login, string passwrod)
        {
            Cookies = new CookieCollection();

            SetCookies(login, passwrod);

            do
            {
                try
                {
                    if (Login())
                    {
                        VisitTo("****");
                    }
                }
                catch (System.Net.WebException e) // Internet connect error/drop
                {
                    Console.WriteLine("Internet error");
                }
                System.Threading.Thread.Sleep(60000);
            } while (true);

        }
        public bool VisitTo(string addr)
        {
            bool result = true;

            HttpWebRequest clientVisit = CreateWebRequest(addr, Method.GET);
            clientVisit.CookieContainer.Add(Cookies); // add cookies to container

            WebResponse requestVisit = clientVisit.GetResponse();
            using (Stream streamGetData = requestVisit.GetResponseStream())
            {
                using (StreamReader reader = new StreamReader(streamGetData))
                {
                    CsQuery.CQ DOM = CsQuery.CQ.Create(reader); // parse html

                    CsQuery.IDomObject profile = DOM.Find("div").Where(e => e.ClassName == "logininfo").FirstOrDefault(); // find profile block
                    CsQuery.IDomElement profileName = profile?.ChildElements.Where(e => e.Attributes["title"] == "Просмотр профиля").FirstOrDefault(); // find name

                    if (profileName != null)
                    {
                        Console.WriteLine($"User: { profileName.FirstChild }");
                        Console.WriteLine($"Visit: `{ DOM.Find("title").Text() }`"); // return title course
                    }
                    else
                    {
                        Console.WriteLine("ERROR LOGIN");
                    }
                }
            }

            requestVisit.Close();
            return result;
        }
        public bool Login()
        {
            bool result = true;

            HttpWebRequest clientLogin = CreateWebRequest("****", Method.POST);

            string dataRequest = $"{Cookies["username"]}&{Cookies["password"]}";
            SetContentRequest(clientLogin, dataRequest); // set data for login

            HttpWebResponse responseLogin = (HttpWebResponse)clientLogin.GetResponse();
            HttpWebRequest clientAfterLogin = CreateWebRequest(responseLogin.ResponseUri.ToString(), Method.GET);

            if (responseLogin.StatusCode == HttpStatusCode.OK)
            {
                HttpWebResponse responseAfterLogin = (HttpWebResponse)clientAfterLogin.GetResponse(); // response for visit 
                foreach (Cookie cookie in responseLogin.Cookies)
                {
                    Cookies.Add(cookie);
                }

                responseAfterLogin.Close();
            }
            else
            {
                Console.WriteLine(responseLogin.StatusCode);
                Console.ReadKey();

                result = false;
            }

            responseLogin.Close();
            return result;
        }
        private void SetCookies(string login, string password)
        {
            Cookie userLogin = new Cookie("username", login, "/", "****");
            Cookie userPassword = new Cookie("password", password, "/", "****");

            Cookies.Add(userLogin);
            Cookies.Add(userPassword);
        }
        private void SetContentRequest(HttpWebRequest request, string loginData)
        {
            byte[] dataToSend = System.Text.Encoding.UTF8.GetBytes(loginData);
            request.ContentLength = dataToSend.Length;

            using (Stream streamData = request.GetRequestStream())
            {
                streamData.Write(dataToSend, 0, dataToSend.Length);
            }
        }
        private HttpWebRequest CreateWebRequest(string addr, Method method)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(addr);

            request.Method = method == Method.POST ? "POST" : "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            request.CookieContainer = new CookieContainer();

            return request;
        }

        private enum Method
        {
            GET,
            POST
        };
    }
}
