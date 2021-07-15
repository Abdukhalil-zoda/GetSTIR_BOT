using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;


namespace GetSTIR_BOT
{
    class Program
    {
        static TelegramBotClient client = new TelegramBotClient("1353111175:AAHuw6X5spvyMX1crzDV95zqvEHQRDmqZTw");

        static void Main(string[] args)
        {


                /*
                var capcha = GetCaptcha();

                JObject jObj = JObject.Parse(capcha);
                var imgScr = jObj["imgSrc"].ToString();
                var len = imgScr.Length;
                imgScr = imgScr.Substring(24, len - 24);

                Bitmap img = GetImgFromBase64(imgScr);
                img.Save("captcha.png");
                Console.WriteLine("pasport reriasini kiriting(AA)");
                var seria = Console.ReadLine();
                Console.WriteLine("pasport raqamini kiriting(1234567)");
                var num = Console.ReadLine();
                Console.WriteLine("Tug'ilgan sana kiriting(23.02.1994)");
                var data = Console.ReadLine();
                Console.WriteLine(Cookie);
                var response = await POST(seria, num, data);

                JObject obj = JObject.Parse(response);
                Console.Clear();
                Console.WriteLine($"Ф.И.О : {obj["surName"]} {obj["firstName"]} {obj["middleName"]}\nСТИР : {obj["tin"]}\nСТИР берилган сана : {obj["tinDate"]} \nРўйхатга олинган ДСИ : {obj["ns11Name"]}");

                Console.WriteLine("Enter any key...");

                Console.ReadKey(true);
                Console.WriteLine("Thanks for using!");
                await Task.Delay(3000);
                return;*/
                
                client.OnMessage += Client_OnMessage;
                client.StartReceiving();

                Console.Title = client.GetMeAsync().Result.FirstName;

                Console.ReadLine();
                client.StopReceiving();
            
        }
        static Bitmap GetImgFromBase64(string Base64S)
        {
            Byte[] bitmapData = Convert.FromBase64String(FixBase64ForImage(Base64S));
            MemoryStream streamBitmap = new MemoryStream(bitmapData);
            Bitmap bitImage = new Bitmap((Bitmap)Image.FromStream(streamBitmap));

            return bitImage;
        }
        static string Cookie;

        public static object Fiel { get; private set; }

        static string FixBase64ForImage(string Image)
        {
            StringBuilder sbText = new StringBuilder(Image, Image.Length);
            sbText.Replace("\r\n", String.Empty); sbText.Replace(" ", String.Empty);
            return sbText.ToString();
        }
        static async Task<string> POST(string Seria, string PassNumber, string DataOfBirthday)
        {
            var baseAddress = new Uri("https://my.soliq.uz");
            var cookieContainer = new CookieContainer();
            using (var handler = new HttpClientHandler() { CookieContainer = cookieContainer })
            using (var client = new HttpClient(handler) { BaseAddress = baseAddress })
            {
                //Console.WriteLine("Please enter Captcha\nLoading captcha...");

                //Process.Start("cmd", "/c explorer captcha.png");
                //await Task.Delay(1000);
                //var s = Console.ReadLine();
                var values = new Dictionary<string, string>
                {
                    { "passer", Seria.ToLower() },
                    { "pasnum", PassNumber },
                    { "pasdob", DataOfBirthday },
                    { "doccode", "01" },
                    { "captcha", "5268" }
                };
                var content = new FormUrlEncodedContent(values);
                cookieContainer.Add(baseAddress, new Cookie("CAPTCHA_ID", $"{"7F277A855E6B106BF565B241123B3637"}"));
                var result = await client.PostAsync("/searchtin/gettinbydate", content);
                result.EnsureSuccessStatusCode();
                var responseString = await result.Content.ReadAsStringAsync();
                return responseString;
            }

        }
        static string GetCaptcha()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://my.soliq.uz/searchtin/captcha");
            request.CookieContainer = new CookieContainer();
            request.CookieContainer.Add(new Cookie("CAPTCHA_ID", "5BCFBDCE88397D652A59DDE498238D1E", "/", "my.soliq.uz"));

            HttpWebResponse response = null;
            string responseString = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                
                Cookie = response.Headers.Get("Set-Cookie");
                Cookie = Cookie.Split(';')[0].Split('=')[1];
                responseString = new StreamReader(response.GetResponseStream()).ReadToEnd();

            }
            catch (Exception ex)
            {

                throw ex;
            }

            return responseString;
        }

        private async static void Client_OnMessage(object sender, Telegram.Bot.Args.MessageEventArgs e)
        {

            var Chat = e.Message.Chat.Id;
            var From = e.Message.Chat.Id;
            var mess = e.Message.Text;
            var history = string.Format("incoming {0}", e.Message.From.FirstName + $" {From} " + mess);
            File.AppendAllText("History.log", $"{history}\n");
            Console.WriteLine(history);
            try
            {
                if (mess != null)
                {
                    if (e.Message.From.Id == 330520571)
                    {

                        if (e.Message.ReplyToMessage != null)
                        {
                            var quFrom = File.ReadAllLines($".\\ques\\{e.Message.ReplyToMessage.MessageId}")[0];
                            var messId = File.ReadAllLines($".\\ques\\{e.Message.ReplyToMessage.MessageId}")[1];

                            Console.WriteLine($"{quFrom} {messId}");

                            await client.SendTextMessageAsync(long.Parse(quFrom), mess, replyToMessageId: int.Parse(messId));
                            Console.WriteLine("send");
                            return;
                        }
                        
                    }
                    if (mess.Split()[0].ToLower() == "/get")
                    {
                        try
                        {
                            string seria = mess.Split()[1].ToLower();
                            string num = mess.Split()[2].ToLower();
                            string data = mess.Split()[3].ToLower();

                            var response = await POST(seria, num, data);

                            JObject obj = JObject.Parse(response);
                            //Console.Clear();
                            if (obj["surName"].ToString() == "")
                            {
                                await client.SendTextMessageAsync(Chat, "Malumot topilmadi!\nKiritilgan malumotlarni to'g'riligini tekshiring");
                                return;
                            }
                            await client.SendTextMessageAsync(Chat, $"Ф.И.О : {obj["surName"]} {obj["firstName"]} {obj["middleName"]}\nСТИР : {obj["tin"]}\nСТИР берилган сана : {obj["tinDate"]} \nРўйхатга олинган ДСИ : {obj["ns11Name"]}");
                            File.AppendAllText("DB", obj.ToString() + Environment.NewLine);
                        }
                        catch (Exception ex)
                        {
                            var str = $"[{DateTime.Now:dd.MM HH:mm}]\n\t{ex.Message}\n\t{mess}\n";
                            File.AppendAllText("err.log", str);
                            await client.SendTextMessageAsync(Chat, "Malumot topilmadi!\nKiritilgan malumotlarni to'g'riligini tekshiring\n\n /get ishlatish quydagicha:\n /get AA(ya'ni seriya raqami) 1234567(Pasport raqami) 12.03.2000(Tugilgan sana) \nSavol bo'lsa bemalol murojat qiling @Abdukhalil_zoda");
                        }

                    }
                    else if (mess == "/start")
                    {
                        await client.SendTextMessageAsync(Chat, "Salom STIRni olish uchun /get komandasidan foydalaning\nAgar savolingiz bolsa shunchaki yozib yu boring men uni adminlarga yuboraman :)");
                    }
                    else
                    {
                        var message = await client.ForwardMessageAsync(330520571, Chat, e.Message.MessageId);
                        Console.WriteLine(e.Message.MessageId);
                        await client.SendTextMessageAsync(Chat, replyToMessageId: e.Message.MessageId, text: "Adminlarga yuborildi sizga tez orada javob berishadi (;");

                        File.WriteAllLines($".\\ques\\{message.MessageId}", new[] { Chat.ToString(), (e.Message.MessageId).ToString() });
                    }

                    
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                
            }
            
            


        }
    }
}
