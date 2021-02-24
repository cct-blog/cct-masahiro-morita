using blazorTest.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace blazorTest.Client.Services
{
    public static class RoomUtility
    {
        /// <summary>
        /// ルームから指定したユーザーを削除します。
        /// </summary>
        /// <param name="roomId">ユーザーを削除するルームのID</param>
        /// <param name="userEmail">`;`で削除するユーザーのEmailを区切った文字列</param>
        /// <returns></returns>
        public static async Task DeleteUserFromRoom(Guid roomId, string userEmail)
        {
            if (!string.IsNullOrEmpty(userEmail))
            {
                if (!userEmail.Contains(";"))
                {
                    string[] userEmailArray = { userEmail };
                    await RoomUtility.DeleteUserFromRoom(roomId, new List<string>(userEmailArray));
                }
                else
                {
                    await RoomUtility.DeleteUserFromRoom(roomId, userEmail.Split(";").ToList());
                }
            }
        }

        /// <summary>
        /// ルームから指定したユーザーを削除します。
        /// </summary>
        /// <param name="roomId">ユーザーを削除するルームのID</param>
        /// <param name="userEmail">ルームから削除するユーザーのEmailのリスト</param>
        /// <returns></returns>
        public static async Task DeleteUserFromRoom(Guid roomId, List<string> userEmail)
        {
            var httpClient = new HttpClient();
            var url = new System.Text.StringBuilder();
            url.Append("Room/");
            url.Append(roomId);
            url.Append("/User");

            var response = await httpClient.DeleteWithJsonAsync<List<string>>(url.ToString(), userEmail);
            var responseContent = await response.Content.ReadFromJsonAsync<RoomDetail>();

            if (responseContent.Users?.Count == 0)
            {
                url.Clear();
                url.Append("Room/");
                url.Append(roomId);
                await httpClient.DeleteAsync(url.ToString());
            }
        }

        /// <summary>
        /// ルーム内の投稿を更新します。
        /// ただし、ルームの最終アクセス日時は更新しません。
        /// </summary>
        /// <param name="roomId">投稿を取得したいルームのID</param>
        /// <returns>ルーム内の投稿一覧</returns>
        public static async Task<List<Message>> RefreshPostInRoom(Guid roomId)
        {
            var httpClient = new HttpClient();

            var request = new ChatPostPostRequest()
            {
                RoomId = roomId,
                NeedMessageTailDate = DateTime.Now
            };

            var response = await httpClient.GetWithJsonAsync<ChatPostPostRequest>("Post", request);

            return await response.Content.ReadFromJsonAsync<List<Message>>();
        }

        /// <summary>
        /// ルーム内の投稿を更新します。
        /// ルームの最終アクセス日時も更新します。
        /// </summary>
        /// <param name="roomId">投稿を取得したいルームのID</param>
        /// <returns>ルーム内の投稿一覧</returns>
        public static async Task<List<Message>> InitializeRoom(Guid roomId)
        {
            var messages = await RefreshPostInRoom(roomId);

            var httpClient = new HttpClient();
            var url = new System.Text.StringBuilder();
            url.Append("Room/");
            url.Append(roomId);

            await httpClient.PutAsync(url.ToString(), null);

            return messages;
        }
    }
}