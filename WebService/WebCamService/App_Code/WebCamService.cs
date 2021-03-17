using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;

[WebService(Namespace = "www.codeproject.com")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
public class WebCamService : System.Web.Services.WebService
{
    WebCamLibrary.WebCam cam;

    public WebCamService()
    {

        //Uncomment the following line if using designed components 
        //InitializeComponent(); 
        cam = WebCamLibrary.WebCam.NewWebCam();
              
    }

    [WebMethod]
    public byte[] GrabFrame() {
        return cam.GrabFrame();
    }
    [WebMethod]
    public string[] GetConnectedCameras()
    {
        return cam.GetConnectedCameras();
    }
  

    
}
