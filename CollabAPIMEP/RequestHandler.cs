﻿using Autodesk.Revit.UI;
using System;
using System.Threading;

namespace CollabAPIMEP
{
    public class RequestHandler : IExternalEventHandler
    {
        // A trivial delegate, but handy
        //private delegate void DoorOperation(FamilyInstance e);
        RequestMethods helperMethods = null;
        MainViewModel mainViewModel = null;
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

        public RequestHandler(MainViewModel viewModel, FamilyLoadHandler familyLoadHandler)
        {
            mainViewModel = viewModel;
            this.familyLoadHandler = familyLoadHandler;
            if (helperMethods == null)
            {
                helperMethods = new RequestMethods(mainViewModel, familyLoadHandler);
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
                    case RequestId.GetModelUpdates:
                        {
                            helperMethods.GetModelUpdates();
                            break;
                        }
                    case RequestId.ToggleFamilyLoaderEvent:
                        {
                            helperMethods.ToggleFamilyLoaderEvent();
                            break;
                        }
                    case RequestId.ToggleFamilyLoadingEvent:
                        {
                            helperMethods.ToggleFamilyLoadingEvent();
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

        private MainViewModel mainViewModel;
        private FamilyLoadHandler familyLoadHandler;
        public RequestMethods(MainViewModel viewModel, FamilyLoadHandler familyLoadHandler)
        {
            mainViewModel = viewModel;
            this.familyLoadHandler = familyLoadHandler;
        }

        public void ToggleFamilyLoaderEvent()
        {
            if (mainViewModel.LoaderStateText == "Disabled")
            {
                mainViewModel.EnableFamilyLoader();
                //familyLoadHandler.ManualFamilyLoad();
                mainViewModel.LoaderStateText = "Enabled";
            }
            else
            {
                mainViewModel.DisableFamilyLoader();
                //familyLoadHandler.ManualFamilyLoad();
                mainViewModel.LoaderStateText = "Disabled";
            }
        }

        public void ToggleFamilyLoadingEvent()
        {
            if (mainViewModel.LoaderStateText == "Disabled")
            {
                mainViewModel.EnableFamilyLoading();
                //familyLoadHandler.ManualFamilyLoad();
                mainViewModel.LoadingStateText = "Enabled";
            }
            else
            {
                mainViewModel.DisableFamilyLoading();
                //familyLoadHandler.ManualFamilyLoad();
                mainViewModel.LoadingStateText = "Disabled";
            }
        }
        public void GetModelUpdates()
        {
            // can be removed
        }


    }
    public enum RequestId : int
    {
        None = 0,

        GetModelUpdates = 1,

        ToggleFamilyLoaderEvent = 2,

        ToggleFamilyLoadingEvent = 3,
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
