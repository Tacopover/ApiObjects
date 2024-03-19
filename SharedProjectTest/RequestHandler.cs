using Autodesk.Revit.UI;
using System;
using System.Linq;
using System.Threading;

namespace CollabAPIMEP
{
    public class RequestHandler : IExternalEventHandler
    {
        // A trivial delegate, but handy
        //private delegate void DoorOperation(FamilyInstance e);
        RequestMethods helperMethods = null;
        //MainViewModel mainViewModel = null;
        FamilyLoadHandler familyLoadHandler = null;

        // The value of the latest request made by the modeless form 
        private Request m_request = new Request();

        /// <summary>
        /// A public property to access the current request value
        /// </summary>
        public Request Request
        {
            get { return m_request; }
        }

        /// <summary>
        ///   A method to identify this External Event Handler
        /// </summary>
        public String GetName()
        {
            return "FamilyLoadHandler";
        }

        public RequestHandler(FamilyLoadHandler familyLoadHandler, RequestMethods helperMethods)
        {
            this.familyLoadHandler = familyLoadHandler;
            if (this.helperMethods == null)
            {
                this.helperMethods = helperMethods;
            }
        }

        /// <summary>
        ///   The top method of the event handler.
        /// </summary>
        /// <remarks>
        ///   This is called by Revit after the corresponding
        ///   external event was raised (by the modeless form)
        ///   and Revit reached the time at which it could call
        ///   the event's handler (i.e. this object)
        /// </remarks>
        /// 
        public void Execute(UIApplication uiapp)
        {

            try
            {
                switch (Request.Take())
                {
                    case RequestId.None:
                        {
                            return;  // no request at this time -> we can leave immediately
                        }
                    case RequestId.SaveRules:
                        {
                            helperMethods.SaveRules();
                            break;
                        }
                    case RequestId.EnableLoading:
                        {
                            helperMethods.EnableLoading();
                            break;
                        }
                    case RequestId.DisableLoading:
                        {
                            helperMethods.DisableLoading();
                            break;
                        }


                    default:
                        {
                            throw new Exception("Unknown command issued to the RequestHandler");
                        }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.ToString();

                TaskDialog.Show("error", msg);
            }
            finally
            {
                //RevitCommand.mainEntry.WakeFormUp();
            }

            return;
        }

    }

    public class RequestMethods
    {

        private FamilyLoadHandler familyLoadHandler;
        public RequestMethods(FamilyLoadHandler familyLoadHandler)
        {
            this.familyLoadHandler = familyLoadHandler;
        }

        public void EnableLoading()
        {
            familyLoadHandler.EnableFamilyLoading();
        }
        public void DisableLoading()
        {
            familyLoadHandler.DisableFamilyLoading();
        }

        public void SaveRules()
        {
            familyLoadHandler.SaveRules();
        }


    }
    public enum RequestId : int
    {
        None = 0,

        EnableLoading = 1,

        DisableLoading = 2,

        SaveRules = 3,
    }


    public class Request
    {

        private int m_request = (int)RequestId.None;


        public RequestId Take()
        {
            return (RequestId)Interlocked.Exchange(ref m_request, (int)RequestId.None);
        }

        public void Make(RequestId request)
        {
            Interlocked.Exchange(ref m_request, (int)request);
        }
    }
}
