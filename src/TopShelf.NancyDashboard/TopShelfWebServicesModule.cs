namespace Topshelf.NancyDashboard
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using Ionic.Zip;
    using Logging;
    using Messages;
    using Model;
    using Nancy;
    using Stact;
    using Stact.MessageHeaders;

    public class TopshelfWebServicesModule : NancyModule
    {
        readonly ILog _log = Logger.Get("Topshelf.WebControl.TopShelfControlModule");
        public dynamic Model = new ExpandoObject();

        public TopshelfWebServicesModule()
        {
            var serviceCoordinator = TinyIoC.TinyIoCContainer.Current.Resolve<IServiceChannel>();

            Get["/"] = x =>
                {
                    Model.Title = "Topshelf";
                    Model.Services = GetServices(serviceCoordinator);

                    var configuration = TinyIoC.TinyIoCContainer.Current.Resolve<DashboardConfiguration>();
                    Model.IsPackageUploadsEnabled = configuration.EnablePackageUploads;

                    return View["dashboard.html", Model];
                };

            Get["/services"] = x =>
                {
                    return Response.AsJson(GetServices(serviceCoordinator));
                };

            Get["/service/{name}/start"] = parms =>
            {
                string name = parms.name;

                serviceCoordinator.Send(new StartService(name));

                return HttpStatusCode.OK;
            };

            Get["/service/{name}/stop"] = parms =>
            {
                string name = parms.name;

                serviceCoordinator.Send(new StopService(name));

                return HttpStatusCode.OK;
            };

            Get["/service/{name}/unload"] = parms =>
            {
                string name = parms.name;

                serviceCoordinator.Send(new UnloadService(name));

                return HttpStatusCode.OK;
            };

            Post["/service/upload"] = parms =>
            {
                var uploadedFile = this.Request.Files.FirstOrDefault();

                if (uploadedFile != null)
                {
                    var configuration = TinyIoC.TinyIoCContainer.Current.Resolve<DashboardConfiguration>();
                    if (!Directory.Exists(configuration.PackageStore))
                    {
                        try
                        {
                            Directory.CreateDirectory(configuration.PackageStore);
                        }
                        catch (Exception ex)
                        {
                            _log.ErrorFormat("Failed to create the Package store at path: " + configuration.PackageStore, ex);
                            return HttpStatusCode.InternalServerError;
                        }
                    }

                    var packagePath = Path.Combine(configuration.PackageStore, uploadedFile.Name);
                    using (var file = File.OpenWrite(packagePath))
                    {
                        uploadedFile.Value.CopyTo(file);
                    }

                    var targetPath = Path.Combine(configuration.ServicesLocation, Path.GetFileNameWithoutExtension(uploadedFile.Name));
                    UnpackPackage(packagePath, targetPath);
                }

                return HttpStatusCode.OK;
            };


            Get["/images/{file}"] = @params => Response.AsImage("." + Request.Path);
            
            Get["/styles/{file}"] = @params => Response.AsCss("." + Request.Path);

            Get["/scripts/{file}"] = @params => Response.AsJs("." + Request.Path);
        }

        private IEnumerable<dynamic> GetServices(IServiceChannel serviceCoordinator)
        {
            var handle = new AutoResetEvent(false);

            IEnumerable<dynamic> report = null;

            AnonymousActor.New(inbox =>
            {
                serviceCoordinator.Send<Request<ServiceStatus>>(new RequestImpl<ServiceStatus>(inbox, new ServiceStatus()));

                inbox.Receive<Response<ServiceStatus>>(response =>
                {
                    report = from service in response.Body.Services
                             select new
                                 {
                                     Name = Uri.EscapeUriString(service.Name),
                                     service.ServiceType,
                                     service.CurrentState,
                                     Action = (service.CurrentState == "Running") ? "stop" : "start"
                                 };

                    handle.Set();

                }, TimeSpan.FromSeconds(10), () =>
                {
                    report = new[] { new { Name = "Failed to get Services list" } };

                    handle.Set();
                });
            });

            handle.WaitOne();

            return report;
        }

        private bool UnpackPackage(string sourcePath, string targetpath)
        {
            try
            {
                if (Directory.Exists(targetpath))
                {
                    _log.Error("The target Package location is in use. Consider removing folder: " + targetpath);
                    return false;
                }

                Directory.CreateDirectory(targetpath);

                using (var zip1 = ZipFile.Read(sourcePath))
                {
                    zip1.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
                    zip1.ExtractAll(targetpath);
                }

                return true;
            }
            catch (Exception ex)
            {
                _log.Error("Failed to unpack Package", ex);
                return false;
            }
        }
    }
}