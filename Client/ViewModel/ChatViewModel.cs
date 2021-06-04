using blazorTest.Client.Models;
using Oniqys.Blazor.ViewModel;

namespace blazorTest.Client.ViewModel
{
    public class ChatViewModel : ContentBase
    {
        private ChatModel _model = new();

        /// <summary>
        /// 全ユーザーと（入室中情報付き）
        /// </summary>
        public ContentCollection<Selectable<UserViewModel>> Users { get; set; } = new();

        public Selectable UserListOpened { get; } = new Selectable { IsSelected = false, IsEnabled = true };

    }
}
