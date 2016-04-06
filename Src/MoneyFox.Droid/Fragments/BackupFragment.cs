using Android.Runtime;
using MoneyFox.Shared.ViewModels;
using MvvmCross.Droid.Support.V7.Fragging.Attributes;

namespace MoneyFox.Droid.Fragments
{
    [MvxFragment(typeof (MainViewModel), Resource.Id.content_frame)]
    [Register("moneymanager.droid.fragments.BackupFragment")]
    public class BackupFragment : BaseFragment<BackupViewModel>
    {
        protected override int FragmentId => Resource.Layout.fragment_backup;
    }
}