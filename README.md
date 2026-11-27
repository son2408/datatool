# DataTool

Ứng dụng desktop WPF (.NET Framework 4.8) để chạy các tác vụ xử lý dữ liệu.

## Cấu trúc solution

| Project | Vai trò |
|---|---|
| `DataTool.UI.Themes` | Theme dùng chung (màu, font, style) dựa trên MaterialDesignInXamlToolkit. Nạp bằng `Themes/Theme.xaml`. |
| `DataTool.UI.Controls` | Control dùng lại: `LogViewer`, `ProgressPanel`, `EnvironmentBadge`, `ConnectionStatusIndicator`, `ConfirmDialog`, `PasswordBoxHelper`, converter. |
| `DataTool.App` | Ứng dụng WPF (MVVM, CommunityToolkit.Mvvm). |

Sắp thêm: `DataTool.Core`, `DataTool.Configuration`, `DataTool.VBDLIS`, `DataTool.ConsoleDemo`.

## Trạng thái

Bước 1, dựng giao diện: App đang dùng **dữ liệu mẫu** (3 môi trường Dev/Test/Production,
2 tác vụ VBDLIS giả lập, kiểm tra kết nối giả lập). Các phần đánh dấu "TẠM THỜI" trong
`DataTool.App/Models` và `DataTool.App/Services/Sample*`, `Fake*` sẽ được thay bằng
Core và Configuration ở các bước sau.

## Build

Cần Windows với Visual Studio 2022 (hoặc .NET SDK 6+):

```
dotnet build DataTool.sln
```

Chạy `src/DataTool.App/bin/Debug/net48/DataTool.exe`.
