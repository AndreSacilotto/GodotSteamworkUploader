using Godot;
using System.IO;
using Steamworks;

namespace SteamworkUploader;

public partial class Uploader : Control
{
    [Export] private Button submitBtn;

    [Export] private LineEdit appIdInput;
    [Export] private LineEdit workshopIdInput;

    [Export] private LineEdit titleInput;
    [Export] private TextEdit descriptionInput;
    [Export] private LineEdit tagsInput;
    [Export] private LineEdit changelogInput;

    [ExportGroup("Log Dialog")]
    [Export] private Window logDialog;
    [Export] private TextEdit logLabel;
    [Export] private Button logBtn;

    [ExportGroup("Folder Path Dialog")]
    [Export] private LineEdit folderInput;
    [Export] private Button folderSelect;
    [Export] private FileDialog folderDialog;

    [ExportGroup("Image Dialog")]
    [Export] private LineEdit imagePathInput;
    [Export] private Button imageSelect;
    [Export] private FileDialog imageFileDialog;
    [Export] private Button imagePreviewBtn;
    [Export] private Window imagePreviewDialog;
    [Export] private TextureRect imagePreviewRect;

    public void Log(string message) => logLabel.Text += message + '\n';

    public override void _Ready()
    {
        submitBtn.Pressed += SubmitBtn_Pressed;
        logBtn.Pressed += LogBtn_Pressed;
        imagePreviewBtn.Pressed += ImagePreviewBtn_Pressed;

        folderSelect.Pressed += FolderSelect_Pressed;
        imageSelect.Pressed += ImageSelect_Pressed;

        folderDialog.DirSelected += FolderDialog_FileSelected;

        imageFileDialog.FileSelected += ImageFileDialog_FileSelected;
        imageFileDialog.FilesDropped += ImageFileDialog_FilesDropped;
    }

    private void ImageFileDialog_FilesDropped(string[] files) { if (files.Length > 0) ImageFileDialog_FileSelected(files[0]); }
    private void ImageFileDialog_FileSelected(string path) => imagePathInput.Text = path;

    private void FolderDialog_FileSelected(string path) => folderInput.Text = path;

    private void LogBtn_Pressed() => logDialog.Popup();
    private void ImageSelect_Pressed() => imageFileDialog.Popup();
    private void FolderSelect_Pressed() => folderDialog.Popup();

    private void ImagePreviewBtn_Pressed()
    {
        if (!IsValidPath(imagePathInput.Text))
            return;

        var img = new Image();
        img.Load(imagePathInput.Text);

        var texture = new ImageTexture();
        texture.SetImage(img);

        imagePreviewRect.Texture = texture;

        imagePreviewDialog.Popup();
    }

    private async void SubmitBtn_Pressed()
    {
        Log("Submitting");

        if (uint.TryParse(appIdInput.Text, out var appId))
            return;

        Log($"AppId: {appId}");

        if (ulong.TryParse(workshopIdInput.Text, out var workshopId))
            workshopId = 0;
        var publishId = new Steamworks.Data.PublishedFileId { Value = workshopId };
        var first = workshopId == 0;

        Log("WorkshopId: " + (first ? workshopId : nameof(first)));

        Log("Init Steam");
        SteamClient.Init(appId, true);

        Steamworks.Ugc.Editor item = first ? Steamworks.Ugc.Editor.NewCommunityFile : new Steamworks.Ugc.Editor(publishId);

        if (!string.IsNullOrWhiteSpace(titleInput.Text))
            item.WithTitle(titleInput.Text);

        if (!string.IsNullOrWhiteSpace(tagsInput.Text))
        {
            string[] parts = tagsInput.Text.Split(',');
            foreach (string part in parts)
                item.WithTag(part.Trim());
        }

        if (!string.IsNullOrWhiteSpace(descriptionInput.Text))
            item.WithDescription(descriptionInput.Text);

        if (!string.IsNullOrWhiteSpace(changelogInput.Text))
            item.WithChangeLog(changelogInput.Text);

        var folder = LoadFolder(folderInput.Text);
        if (folder is not null)
            item.WithContent(folder);

        if (IsValidPath(imagePathInput.Text))
            item.WithPreviewFile(imagePathInput.Text);

        //item.WithMetaData(metadataInput.Text);
        //item.WithPrivateVisibility();

        Log("Start Upload");
        var progress = new ProgressClass((x) => Log("Uploading: " + x));
        var result = await item.SubmitAsync(progress);

        SteamClient.Shutdown();
        Log("Shutdown Steam");
    }

    private static DirectoryInfo LoadFolder(string folderPath)
    {
        if (IsValidPath(folderPath))
            return null;
        var dirInfo = new DirectoryInfo(folderPath);
        if (dirInfo.Exists)
            return dirInfo;
        return null;
    }

    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;
        char[] invalidChars = Path.GetInvalidPathChars();
        foreach (char c in path)
            for (int i = 0; i < invalidChars.Length; i++)
                if (c == invalidChars[i])
                    return false;
        return true;
    }

}
