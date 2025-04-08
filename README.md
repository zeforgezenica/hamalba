**Project Setup Guide**

Cloning and Setting Up the Project

When you clone the project or download it from our GitHub repository, open it in Visual Studio.
Also, install the MySQL.Data package within the project.

**Dependencies**

To install all required NuGet packages, run the following commands in the terminal:

* dotnet tool install --global dotnet-ef

* dotnet restore

**Visual Studio Requirements**

If you are using Visual Studio IDE, version 17.12 or higher is required for compatibility with .NET 9.0.

**Required Software**

.NET 9.0

MySQL 8.0.41.0

*Download MySQL Installer:* https://dev.mysql.com/get/Downloads/MySQLInstaller/mysql-installer-community-8.0.41.0.msi

During installation, ensure to add MySQL Server.

*MySQL Installation Guide*

* Open MySQL Installer.

![alt text](readme-images/image.png)

* Click on Next.

![alt text](readme-images/image-1.png)

* Add the following components to the installation:

* Server (version 8.0.41)

* Workbench (version 8.0.41)

* Shell (version 8.0.41)

* Router (version 8.0.41)

![alt text](readme-images/image-2.png)

* Click Execute and wait for the installation to complete.

![alt text](readme-images/image-3.png)

* Once installed, click Next.

![alt text](readme-images/image-4.png)

* Proceed with individual product configuration.

![alt texet](readme-images/image-5.png)

* Select the options as shown in the images and click Next.

![alt text](readme-images/image-6.png)

* Click Next.

![alt text](readme-images/image-7.png)

* Set 'root' as the password.

![alt text](readme-images/image-8.png)

* Select the options as shown in the images and click Next.

![alt text](readme-images/image-9.png)

* Grant necessary access and click Next.

![alt text](readme-images/image-10.png)

* Click Execute to apply settings.

![alt text](readme-images/image-11.png)

* Follow the on-screen steps to complete the configuration, click Next.

![alt text](readme-images/image-12.png)

* Follow the on-screen steps to complete the configuration click Finish.

![alt text](readme-images/image-13.png)

* Proceed with individual product configuration, click Next.

![alt text](readme-images/image-14.png)

* Username: root, Password: root, click Next.

![alt text](readme-images/image-15.png)

* Click Execute.

![alt text](readme-images/image-16.png)

![alt text](readme-images/image-17.png)

* Finally, click Finish.

**Running the Project**

Once everything is set up, run your project by clicking the HTTPS option in the top section of Visual Studio.

[ Diagrams ]

	Use Case: https://online.visual-paradigm.com/share.jsp?id=323931383736382d3137

	Activity: https://online.visual-paradigm.com/share.jsp?id=323931383736382d3138

**Additional Instructions for Linux and Mac**

*Installing .NET on Linux*

* Install the .NET SDK using the package manager for your distribution:

* Ubuntu/Debian:

sudo apt-get update && sudo apt-get install -y dotnet-sdk-9.0

* Fedora:

sudo dnf install dotnet-sdk-9.0

* Arch Linux:

sudo pacman -S dotnet-sdk

* Verify installation:

dotnet --version

* Installing MySQL on Linux

* Install MySQL Server:

sudo apt update && sudo apt install mysql-server

* Start and enable MySQL service:

sudo systemctl start mysql
sudo systemctl enable mysql

* Secure your installation:

sudo mysql_secure_installation

* Log in to MySQL:

mysql -u root -p

*Installing .NET on macOS*

* Install Homebrew (if not installed):

/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"

* Install .NET SDK:

brew install dotnet-sdk

* Verify installation:

dotnet --version

*Installing MySQL on macOS*

* Install MySQL using Homebrew:

brew install mysql

* Start MySQL service:

brew services start mysql

* Set up root password:

mysql_secure_installation

* Log in to MySQL:

mysql -u root -p

Now, follow the same steps to restore dependencies and run your project!

This guide ensures smooth setup across Windows, Linux, and macOS.


