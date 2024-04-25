# Publish

Set of pipeline and logic to apply when publishing

## Publishing `deb` files

We're using the `debhelper` from the `dpkg-dev` package to create our `deb` files.

To install `debhelper`, follow below:

```bash
# Make sure dpkg-dev is installed
sudo apt install dpkg-dev -y
```

### `devtoys.cli`

```bash

# Build DevToys.CLI DevToys.Tools
./quick-build-cli.sh

# Now build an unsigned debian package
cd packaging/deb/devtoys.cli
dpkg-buildpackage -b -uc -us -D

# Test it out by installing the deb file
sudo dpkg -i ../devtoys.cli_<VERSION-IN-CHANGELOG>_all.deb

# Check installed folders and binaries
ls -l /opt/devtoys/devtoys.cli
ls -l /usr/bin/{devtoys.cli,DevToys.cli}*

# try to do a sample run
devtoys.cli --help
devtoys.cli li

# Now uninstall if you want
sudo dpkg -r devtoys.cli
```

### `devtoys.gui`

```bash

# Build DevToys.CLI DevToys.Tools
./quick-build-gui.sh

# Now build an unsigned debian package
cd packaging/deb/devtoys.gui
dpkg-buildpackage -b -uc -us -D

```

The `devtoys.gui` package depends on `libwebkitgtk-6.0-4`.

Hence, to install it on Debian/Ubuntu, we need to make sure the dependencies
are also pre-installed or install its dependencies when installing the `deb`.

```bash
# Test it out by installing the deb file
sudo apt install ./devtoys.gui_<VERSION-IN-CHANGELOG>_all.deb
```

Once installed, check installed folders and binaries:

```bash
ls -l /opt/devtoys/devtoys.gui
ls -l /usr/bin/{devtoys,DevToys}*

# Try to open from terminal
devtoys
```

Now, use any application launcher (Unity, Rofi, dmenu etc) to run DevToys.

> It should show DevToys icon

```bash
# Now uninstall if you want
sudo apt remove devtoys.gui

# If you also want to remove the libwebkitgtk dependency
sudo apt autoremove 
```

### `devtoys` (meta package) --> __TODO__
