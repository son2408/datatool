# DataTool

Ứng dụng desktop WPF (.NET Framework 4.8) để chạy các tác vụ xử lý dữ liệu.

## Cấu trúc solution

| Project | Vai trò |
|---|---|
| `DataTool.UI.Themes` | Theme dùng chung (màu, font, style) dựa trên MaterialDesignInXamlToolkit. Nạp bằng `Themes/Theme.xaml`. |
| `DataTool.UI.Controls` | Control dùng lại: `LogViewer`, `ProgressPanel`, `EnvironmentBadge`, `ConnectionStatusIndicator`, `ConfirmDialog`, `PasswordBoxHelper`, converter. |
| `DataTool.App` | Ứng dụng WPF (MVVM, CommunityToolkit.Mvvm). |
| `DataTool.Core` | ITask, TaskBase, TaskRegistry (tự quét tác vụ), TaskRunner (chạy nền, huỷ, ghi log file), `ProvinceProfile` (cấu hình một tỉnh). netstandard2.0, không phụ thuộc giao diện. |
| `DataTool.Configuration` | Đọc/ghi file cấu hình tỉnh `.yml` (giữ nguyên comment và thứ tự khoá khi lưu), thiết lập người dùng (`settings.yml`). netstandard2.0. |

Sắp thêm: `DataTool.VBDLIS`, `DataTool.ConsoleDemo`.

## Cấu hình tỉnh

Mỗi tỉnh là một file `.yml` trong một thư mục (chọn trong màn hình Cấu hình), ví dụ `gla_64.yml`:

```yaml
service_key: "GLA"
repository_backend: "VDFS"
#solr
solr_uri: "http://host:8696/solr"
solr_key: "GLALIS"
...
```

Thư mục đang dùng và tỉnh đang chọn được ghi nhớ trong `%AppData%\DataTool\settings.yml`.
Xoá một tỉnh trong app sẽ chuyển file vào thư mục con `_deleted`, không xoá hẳn.

## Trạng thái

- Cấu hình tỉnh: đọc/ghi file `.yml` thật.
- Tác vụ: 2 tác vụ VBDLIS **giả lập** (`DataTool.App/Services/SampleTaskCatalog.cs`), sẽ chuyển sang
  project `DataTool.VBDLIS` (class implement `ITask`) ở bước sau.

## Build

Cần Windows với Visual Studio 2022 (hoặc .NET SDK 6+):

```
dotnet build DataTool.sln
```

Chạy `src/DataTool.App/bin/Debug/net48/DataTool.exe`.

## Ảnh chụp giao diện

`DataTool.exe --screenshots <thư mục>` mở các màn hình với dữ liệu mẫu và lưu PNG.
Trên GitHub Actions, bước này chỉ chạy khi commit có chữ `[screenshots]` hoặc chạy workflow thủ công;
ảnh được giữ 1 ngày.
