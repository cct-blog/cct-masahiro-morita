using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ChatApp.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;
using ChatApp.Client.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Oniqys.Blazor.ViewModel;
using static ChatApp.Client.Shared.MainLayout;

namespace ChatApp.Client.ViewModel
{
    public class IndexViewModel : ContentBase
    {
        private readonly IndexModel _model;

        public ContentCollection<RoomModel> Rooms { get; } = new();

        private readonly AuthenticationStateProvider _authenticationStateProvider;

        private bool _isLoggedIn;

        /// <summary>
        /// ログイン状態
        /// </summary>
        public bool IsLoggedIn
        {
            get => _isLoggedIn;
            set
            {
                if (ValueChangeProcess(ref _isLoggedIn, value) && value)
                {
                    InitializeRoomList();
                }
            }
        }

        public IndexViewModel(IndexModel indexModel, AuthenticationStateProvider authenticationStateProvider)
        {
            _model = indexModel;
            _authenticationStateProvider = authenticationStateProvider;
        }

        private void InitializeRoomList()
        {
            // 同期処理内で非同期処理を管理する手法。
            Task initialized = Task.CompletedTask;

            _model.RoomListChanged += async (s, e) => { await initialized; OnRoomChanged(e); };
            initialized = _model.Initialize(_authenticationStateProvider);
        }

        private void OnRoomChanged(RoomModel[] rooms)
        {
            var oldRoomIds = Rooms.Select(each => each.RoomId).ToHashSet();
            var newRoomIds = rooms.Select(each => each.RoomId).ToHashSet();

            var updateRoomIds = Rooms
                .Select(each => each.RoomId)
                .Intersect(rooms.Select(each => each.RoomId).ToHashSet());

            var deleteRoomIds = oldRoomIds.Except(updateRoomIds);
            var addRoomIds = newRoomIds.Except(updateRoomIds);

            foreach (var updateRoomId in updateRoomIds)
            {
                var target = Rooms.FirstOrDefault(each => each.RoomId == updateRoomId);
                target = rooms.FirstOrDefault(each => each.RoomId == updateRoomId);
            }

            foreach (var deleteRoomId in deleteRoomIds)
            {
                var target = Rooms.FirstOrDefault(each => each.RoomId == deleteRoomId);
                Rooms.Remove(target);
            }

            foreach (var addRoomId in addRoomIds)
            {
                var target = rooms.FirstOrDefault(each => each.RoomId == addRoomId);
                Rooms.Add(target);
            }

            OnPropertyChanged(nameof(Rooms));
        }

        public async Task CreateRoomAsync(string roomName, List<string> userEmails)
            => await _model.CreateRoomAsync(roomName, userEmails, _authenticationStateProvider);
    }
}