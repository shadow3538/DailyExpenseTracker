using Microsoft.Maui.Controls.Shapes;

namespace DailyExpenseTracker;

public partial class ProfileEditPage : ContentPage
{
    static readonly string[] Genders = { "পুরুষ", "নারী", "অন্যান্য" };
    static readonly string[] Bloods = { "A+", "A-", "B+", "B-", "AB+", "AB-", "O+", "O-" };

    string _gender = "";
    string _blood = "";
    string? _tempPhoto;
    bool _removePhoto;

    public ProfileEditPage()
    {
        InitializeComponent();

        ToolbarItems.Add(new ToolbarItem
        {
            Text = L.T("বন্ধ করুন"),
            Command = new Command(async () => await CloseAsync())
        });

        NameEntry.Text = Profile.Name;
        PhoneEntry.Text = Profile.Phone;
        EmailEntry.Text = Profile.Email;
        JobEntry.Text = Profile.Occupation;
        AddressEntry.Text = Profile.Address;
        BioEditor.Text = Profile.Bio;
        _gender = Profile.Gender;
        _blood = Profile.BloodGroup;

        DobPick.MinimumDate = new DateTime(1900, 1, 1);
        DobPick.MaximumDate = DateTime.Today;
        var dob = Profile.Dob;
        DobPick.Date = dob ?? new DateTime(2000, 1, 1);
        DobSwitch.IsToggled = dob != null;
        DobBox.IsVisible = dob != null;

        BuildChips(GenderBox, Genders, () => _gender, v => _gender = v);
        BuildChips(BloodBox, Bloods, () => _blood, v => _blood = v);
        RefreshAvatar();
        NameEntry.Unfocused += (_, _) => RefreshAvatar();
    }

    protected override bool OnBackButtonPressed()
    {
        _ = CloseAsync();
        return true;
    }

    async Task CloseAsync()
    {
        try { await Navigation.PopModalAsync(); } catch { }
    }

    void RefreshAvatar()
    {
        string? path = _removePhoto ? null : (_tempPhoto ?? Profile.PhotoPath);
        AvatarHost.Content = Ui.AvatarFrom(path, NameEntry.Text, 112, ring: false);
        RemoveBtn.IsVisible = path != null;
    }

    async void OnPickPhoto(object? sender, EventArgs e)
    {
        try
        {
            var opts = MediaPicker.Default.IsCaptureSupported
                ? new[] { L.T("গ্যালারি থেকে বাছুন"), L.T("ক্যামেরায় তুলুন") }
                : new[] { L.T("গ্যালারি থেকে বাছুন") };
            var pick = await DisplayActionSheet(L.T("প্রোফাইল ছবি"), L.T("বাতিল"), null, opts);
            if (pick == null || pick == L.T("বাতিল")) return;

            FileResult? file = pick.StartsWith(L.T("ক্যামেরা"))
                ? await MediaPicker.Default.CapturePhotoAsync()
                : await MediaPicker.Default.PickPhotoAsync();
            if (file == null) return;

            var path = await PhotoHelper.PrepareAsync(file);
            if (path == null)
            {
                Ui.Toast(L.T("ছবিটি পড়া যায়নি, অন্য ছবি চেষ্টা করুন"));
                return;
            }

            if (_tempPhoto != null) { try { File.Delete(_tempPhoto); } catch { } }
            _tempPhoto = path;
            _removePhoto = false;
            RefreshAvatar();
        }
        catch
        {
            Ui.Toast(L.T("ছবি বাছাই করা যায়নি (ক্যামেরা/গ্যালারির অনুমতি দেখুন)"));
        }
    }

    void OnRemovePhoto(object? sender, EventArgs e)
    {
        if (_tempPhoto != null) { try { File.Delete(_tempPhoto); } catch { } }
        _tempPhoto = null;
        _removePhoto = true;
        RefreshAvatar();
    }

    void OnDobToggled(object? sender, ToggledEventArgs e) => DobBox.IsVisible = e.Value;

    static void BuildChips(FlexLayout box, string[] items, Func<string> get, Action<string> set)
    {
        box.Children.Clear();
        foreach (var name in items)
        {
            var selected = get() == name;
            var chip = new Border
            {
                Padding = new Thickness(16, 9),
                Margin = new Thickness(0, 0, 8, 8),
                BackgroundColor = selected ? Ui.Primary : Ui.Gray,
                Stroke = selected ? Ui.Primary : Ui.Line,
                StrokeThickness = 1,
                StrokeShape = new RoundRectangle { CornerRadius = new CornerRadius(20) },
                Content = new Label
                {
                    Text = L.T(name),
                    FontSize = 15,
                    FontAttributes = selected ? FontAttributes.Bold : FontAttributes.None,
                    TextColor = selected ? Colors.White : Ui.Ink
                }
            };
            var n = name;
            chip.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    set(get() == n ? "" : n);
                    BuildChips(box, items, get, set);
                })
            });
            box.Children.Add(chip);
        }
    }

    async void OnSave(object? sender, EventArgs e)
    {
        try
        {
            var email = (EmailEntry.Text ?? "").Trim();
            if (email.Length > 0 && (!email.Contains('@') || !email.Contains('.')))
            {
                await DisplayAlert(L.T("সমস্যা"), L.T("ইমেইল ঠিকভাবে লিখুন (যেমন: name@example.com)"), L.T("ঠিক আছে"));
                return;
            }

            Profile.Name = NameEntry.Text ?? "";
            Profile.Phone = PhoneEntry.Text ?? "";
            Profile.Email = email;
            Profile.Occupation = JobEntry.Text ?? "";
            Profile.Address = AddressEntry.Text ?? "";
            Profile.Bio = BioEditor.Text ?? "";
            Profile.Gender = _gender;
            Profile.BloodGroup = _blood;
            Profile.Dob = DobSwitch.IsToggled ? DobPick.Date.Date : null;
            Profile.CommitPhoto(_tempPhoto, _removePhoto);
            _tempPhoto = null;

            Profile.RaiseChanged();
            Ui.Toast(L.T("✔ প্রোফাইল সেভ হয়েছে"));
            await CloseAsync();
        }
        catch { }
    }
}
