# MVVM Design Pattern

<img src='../Assets/Images/mvvm.png' alt='A Picture describing MVVM Design Pattern'>

### Folder Structure:
1. **Assets**: Store UI resources like images, styles, or themes.
2. **DataService**: Handle all data interactions such as saving and retrieving folder descriptions, managing the config.json file, and network communication with the server.
3. **Dictionaries**: Store any application-wide resource dictionaries for WPF, such as style templates.
4. **Models**: Define the data structure of the entities, such as Requester, Responder, File, Folder, and ConnectionStatus.
5. **Services**: Handle specific business logic like file comparison (timestamps), cloning operations, and connection status.
6. **ViewModels**: Implement ViewModel classes corresponding to the views. Bind properties and commands to the UI.
7. **Views**: Your XAML files will be here. These will define the UI for the Client, Requester, and Responder interfaces.

### Design Overview:

#### **Models**
- **RequesterModel**: Contains the folder information, file comparison logic, and selection properties.
- **ResponderModel**: Holds folder paths and file info, managing access requests.
- **FileModel**: Defines properties like file path, size, timestamp, and whether it’s checked for synchronization.
- **ConnectionStatusModel**: Tracks if the Requester and Responder are connected, with flags like `IsConnected`, `IsCloning`, etc.

#### **ViewModels**
- **MainViewModel**: Controls the main operations like connecting to the server, selecting folders, and starting the cloning process. It binds to commands like `StartCloningCommand` and `ConnectCommand`.
- **RequesterViewModel**: Manages the folder selection UI, and the display of file lists with checkboxes. It handles the automatic selection of files and binds the `StartCloningCommand` to the cloning operation.
- **ResponderViewModel**: Controls the acceptance or rejection of requests from the server. It ensures that other actions are disabled until a decision is made.

#### **Views (XAML)**
- **MainWindow.xaml**: Your main UI where the user initiates connections and starts the process.
- **RequesterView.xaml**: Contains a list of folders and files with checkboxes, allowing users to select which files to clone.
- **ResponderView.xaml**: Displays the incoming request and allows the user to accept or reject.

#### **Services**
- **FileSyncService**: Implements the actual file comparison (using timestamps) and the cloning process.
- **ConnectionService**: Manages the connection between clients and the server.
- **LoggingService**: Keeps logs of the synchronization and requests for auditing purposes.

### Config Management
Each client has a `config.json` file to store folder descriptions and paths. You can use the `Json.NET` library (`Newtonsoft.Json`) to easily serialize/deserialize JSON data.