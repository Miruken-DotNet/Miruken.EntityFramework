namespace Miruken.EntityFramework.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.NetworkInformation;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Threading.Tasks;
    using Docker.DotNet;
    using Docker.DotNet.Models;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    public abstract class DockerDatabaseSetup : DatabaseSetup
    {
        private readonly string _image;
        private readonly string _tag;
        private readonly string _imageName;
        private readonly int _internalPort;
        private IDockerClient _docker;
        private CreateContainerResponse _containerResponse;

        protected DockerDatabaseSetup(string image, string tag, int internalPort)
        {
            _image        = image ?? throw new ArgumentNullException(nameof(image));
            _tag          = tag;
            _imageName    = string.IsNullOrEmpty(tag) ? image : $"{image}:{tag}";
            _internalPort = internalPort;
        }
        
        protected virtual string ContainerPrefix => "miruken-tests";
        
        public override async ValueTask Setup(
            ConfigurationBuilder configuration,
            IServiceCollection   services)
        {
            var dockerEndpoint = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                ? new Uri("npipe://./pipe/docker_engine")
                : new Uri("unix:///var/run/docker.sock");

            _docker = new DockerClientConfiguration(dockerEndpoint).CreateClient();

            await PullImage();
                
            var externalPort = GetAvailableHostPort(10000, 12000);
            
            // Create container from image
            var parameters = ConfigureContainer(configuration, externalPort);
            parameters.Name = $"{ContainerPrefix}-{Guid.NewGuid()}";
            parameters.ExposedPorts = new Dictionary<string, EmptyStruct>
            {
                [$"{_internalPort}"] = new EmptyStruct()
            };
            parameters.HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    [$"{_internalPort}/tcp"] = new[]
                    {
                        new PortBinding
                        {
                            HostIP   = "0.0.0.0",
                            HostPort = $"{externalPort}"
                        }
                    }
                }
            };
            parameters.Image = _imageName;
            
            var container = await _docker.Containers.CreateContainerAsync(parameters);

            if (!await _docker.Containers.StartContainerAsync(
                container.ID, new ContainerStartParameters
                {
                    DetachKeys = $"d={_image}"
                }))
            {
                throw new Exception($"Could not start container: {container.ID}");
            }

            var count = 10;
            Thread.Sleep(5000);
            var containerStat = await _docker.Containers.InspectContainerAsync(container.ID);
            while (!containerStat.State.Running && count-- > 0)
            {
                Thread.Sleep(1000);
                containerStat = await _docker.Containers.InspectContainerAsync(container.ID);
            }

            _containerResponse = container;
        }
        
        protected abstract CreateContainerParameters ConfigureContainer(
            ConfigurationBuilder configuration, int externalPort);

        private async Task PullImage()
        {
            // look for image
            var images = await _docker.Images.ListImagesAsync(new ImagesListParameters
            {
                MatchName = _imageName
            }, CancellationToken.None);

            // Check if container exists
            var pgImage = images.FirstOrDefault();
            if (pgImage == null)
            {
                Debug.WriteLine($"Pulling docker image {_imageName}");
                await _docker.Images.CreateImageAsync(new ImagesCreateParameters()
                {
                    FromImage = _image,
                    Tag       = _tag
                }, null, new Progress<JSONMessage>());
            }
        }

        private static int GetAvailableHostPort(int startRange, int endRange)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpPorts           = ipGlobalProperties.GetActiveTcpListeners();
            var udpPorts           = ipGlobalProperties.GetActiveUdpListeners();

            var result = startRange;

            while ((tcpPorts.Any(x => x.Port == result) ||
                    udpPorts.Any(x => x.Port == result)) &&
                   result <= endRange)
            {
                ++result;
            }

            if (result > endRange)
            {
                throw new InvalidOperationException(
                    $"Unable to find an open port between {startRange} and {endRange}");
            }

            return result;
        }
        
        public override async ValueTask DisposeAsync()
        {
            if (_containerResponse == null) return;
            
            await _docker.Containers.KillContainerAsync(
                _containerResponse.ID, new ContainerKillParameters());

            await _docker.Containers.RemoveContainerAsync(
                _containerResponse.ID, new ContainerRemoveParameters());
            
            _docker?.Dispose();
        }
    }
}