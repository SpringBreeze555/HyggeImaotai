﻿using System.Threading;
using System.Windows;
using Flurl.Http;
using hygge_imaotai.Domain;
using hygge_imaotai.Entity;
using hygge_imaotai.Repository;
using hygge_imaotai.UserInterface.Component;
using Newtonsoft.Json.Linq;

namespace hygge_imaotai.UserInterface.UserControls
{
    /// <summary>
    /// StoreManageUserControl.xaml 的交互逻辑
    /// </summary>
    public partial class StoreManageUserControl
    {
        public StoreManageUserControl()
        {
            InitializeComponent();
            DataContext = new StoreListViewModel();
            RefreshData();
        }

        private void RefreshData()
        {
            var storeListViewModel = (StoreListViewModel)DataContext;
            StoreListViewModel.StoreList.Clear();
            ShopRepository.GetPageData(1, 10,storeListViewModel).ForEach(StoreListViewModel.StoreList.Add);
            // 分页数据
            var total = ShopRepository.GetTotalCount((StoreListViewModel)DataContext);
            var pageCount = total / 10 + 1;
            storeListViewModel.Total = total;
            storeListViewModel.PageCount = pageCount;
        }

        /// <summary>
        /// 刷新Shop数据
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RefreshShopButton_OnClick(object sender, RoutedEventArgs e)
        {
            StoreListViewModel.StoreList.Clear();
            ShopRepository.TruncateTable();

            var responseStr = await "https://static.moutai519.com.cn/mt-backend/xhr/front/mall/resource/get"
                .GetStringAsync();
            var jObject = JObject.Parse(responseStr);
            var shopsUrl = jObject.GetValue("data").Value<JObject>().GetValue("mtshops_pc").Value<JObject>().GetValue("url").Value<string>();
            var shopInnerJson = await shopsUrl.GetStringAsync();

            var shopInnerJObject = JObject.Parse(shopInnerJson);
            var thread = new Thread(() =>
            {
                foreach (var property in shopInnerJObject.Properties())
                {
                    var shopId = property.Name;
                    var nestedObject = (JObject)property.Value;
                    ShopRepository.InsertShop(new StoreEntity(shopId, nestedObject));
                }
            });
            thread.Start();
            thread.Join();
            RefreshData();
        }

        private void ResetButton_OnClick(object sender, RoutedEventArgs e)
        {
            var storeListViewModel = (StoreListViewModel)DataContext;
            storeListViewModel.ShopId = "";
            storeListViewModel.Province = "";
            storeListViewModel.City = "";
            storeListViewModel.Area = "";
            storeListViewModel.CompanyName = "";
        }

        private void SearchButton_OnClick(object sender, RoutedEventArgs e)
        {
            RefreshData();
        }
    }
}
