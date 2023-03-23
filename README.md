# usbip-monitor

A wrapper around usbip providing lifecycle management of the exported devices. Designed for usage in K8s clusters.

This project assumes the kernel module `vhci_driver` is loaded. Use `modprobe vhci_driver` to load the required driver.

If your kernel did not ship with these modules (e.g. the Hyper-V optimized kernel), you can use my other project [`silvenga-docker/usbip`](https://github.com/silvenga-docker/usbip) to compile the required modules and load them into the running kernel (useful as a `DaemonSet` on your K8s cluster).

## Usage

```bash
# Only if needed:
modprobe vhci_driver

# Attach the remote USB device:
docker run -it --rm \
    --privileged \
    -v /usr/lib/linux-tools:/usr/lib/linux-tools/:ro \
    ghcr.io/silvenga/usbip-monitor:master \
    --host
```

Note that `--privileged` must be used. This is a requirement of `usbip` (which is internally used). The read-only `/usr/lib/linux-tools` mount is to locate a distribution provided `usbip` binary on the host.
