# Publish
Set of pipeline and logic to apply when publishing

## Quick Deb build [for test]

```bash
# Make sure dpkg-dev is installed
sudo apt install dpkg-dev -y

# Build DevToys.CLI DevToys.Tools
./quick-build.sh

# Now build an unsigned debian package
cd packaging/deb/devtoys.cli
dpkg-buildpackage -b -uc -us

# Test it out by installing the deb file
sudo dpkg -i ../devtoys.cli_2.0.0-1_all.deb

# Make sure the content is there
ls -l /opt/devtoys/devtoys.cli
ls -l /usr/bin/{devtoys,DevToys}*

# try to do a sample run
devtoys.cli --help
devtoys.cli li

# Now uninstall if you want

sudo dpkg -r devtoys.cli
```


