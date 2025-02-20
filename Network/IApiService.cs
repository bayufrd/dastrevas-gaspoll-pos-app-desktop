using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KASIR.Network
{
    public interface IApiService
    {
        Task<string> Get(string url);
        Task<string> GetMenuByID(string url, string id);
        Task<string> CheckIsOrdered(string url, string id);
        Task<HttpResponseMessage> PostAddMenu(string jsonString, string url);
        Task<HttpResponseMessage> UpdateMenu(string url, string id, string jsonString);
        Task<HttpResponseMessage> DeleteMenu(string url, string id);
        Task<HttpResponseMessage> DeleteCart(string url, string id);
        Task<HttpResponseMessage> DeleteMember(string url);
        Task<string> GetListMenu(string url);
        Task<HttpResponseMessage> CreateCart(string jsonString, string url);
        Task<HttpResponseMessage> CreateMember(string jsonString, string url);
        Task<string> GetCart(string url);
        Task<string> GetItemOnCart(string url);
        Task<HttpResponseMessage> DeleteCart(string url);
        Task<HttpResponseMessage> PayBill(string jsonString, string url);
        Task<string> PayBillTransaction(string jsonString, string url);
        Task<string> SaveBill(string jsonString, string url);
        Task<string> GetMenuDetailByID(string url, string id);
        Task<HttpResponseMessage> UpdateCart(string jsonString, string url);
        Task<string> GetDiscount(string url, string id);
        Task<HttpResponseMessage> UseDiscount(string jsonString, string url);
        Task<string> GetListBill(string url, string id);
        Task<string> GetActiveCart(string url);
        Task<string> GetTransactionRefund(string url);
        Task<string> Refund(string jsonString, string url);
        Task<HttpResponseMessage> inputPin(string jsonString, string url);
        Task<string> GetCicilDetail(string url);
        Task<HttpResponseMessage> cicilRefund(string jsonString, string url);
        Task<string> CetakLaporanShift(string jsonString, string url);
        Task<string> CekShift(string url);

        Task<string> GetPaymentType(string url);
        Task<HttpResponseMessage> EditMember(string jsonString, string url);
        Task<string> GetMember(string url);

        Task<HttpResponseMessage> notifikasiPengeluaran(string jsonString, string url);
        Task<HttpResponseMessage> deleteCart(string jsonString, string url);

        Task<string> Restruk(string url);
        Task<string> SplitBill(string jsonString, string url);
        Task<HttpResponseMessage> SyncTransaction(string jsonString, string url);
    }
}
