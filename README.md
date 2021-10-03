# .NET Core API Photo Gallery
[![LICENCE](https://img.shields.io/npm/l/react)](https://github.com/andersonpereiradossantos/dotnet-core-api-photo-gallery/blob/main/LICENSE)

## About code

Rest API in .NET Core 5 for photo album management.

## Methods

### Albums
* Show all albums : `GET /api/Album/`
* Show album by id: `GET /api/Album/:id`
* Create album: `POST /api/Album/`
* Update album: `PUT /api/Album/:id`
* Delete album: `DELETE /api/Album/:id`

### Photos
* Show all photos : `GET /api/Photo/`
* Show photo by id: `GET /api/Photo/:id`
* Show photos by album id: `GET /api/Photo/:albumId`
* Download photo by id: `GET /api/Photo/DownloadFile/:id/:thumb?`
* Create photo: `POST /api/Photo/:id`
* Update photo: `PUT /api/Photo/:id`
* Delete photo: `DELETE /api/Photo/:id`

# How to run the project

Prerequisites: .NET Core 5 or higher.

```powershell
# Clone repository
git clone https://github.com/andersonpereiradossantos/dotnet-core-api-photo-gallery.git

# Create a folder called wwwroot in the root of your project and a folder called upload within it;

# Change your local database connection string in appsettings.json;

# Restore nuget dependencies:
PM> Update-Package -Reinstall -ProjectName PhotoInfoApi

# Run the migrations to create the database structure:
PM> update-database

# Run the project.
```

## Examples of usage
* [Angular Photo Gallery](https://github.com/andersonpereiradossantos/angular-photo-gallery)

## License
This project is shared under the MIT license. This means you can modify and use it however you want, even for comercial use. If you liked it, consider marking a ⭐️.

## Author

Anderson Pereira dos Santos

[Linkedin](https://www.linkedin.com/in/andersonpereirasantos)

[Github](https://github.com/andersonpereiradossantos)