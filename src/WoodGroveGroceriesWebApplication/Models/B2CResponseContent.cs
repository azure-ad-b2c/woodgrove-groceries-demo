namespace WoodGroveGroceriesWebApplication.Models
{
    using System.Net;
    using System.Reflection;

    public class B2CResponseContent
    {
        public B2CResponseContent(string message, HttpStatusCode status)
        {
            userMessage = message;
            this.status = (int) status;
            version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }

        public string version { get; set; }
        public int status { get; set; }
        public string userMessage { get; set; }
    }
}