using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookShoppingProject.Utility
{
   public static class SD
    {
        //Proc
        public const string GetCoverTypes = "Proc_GetCoverTypes";
        public const string GetCoverType = "Proc_GetCoverType";
        public const string CreateCoverType = "Proc_CreateCoverType";
        public const string UpdateCoverType = "Proc_UpdateCoverType";
        public const string DeleteCoverType = "Proc_DeleteCoverType";
        //Roles
        public const string Role_User_Admin = "Admin";
        public const string Role_User_Employee = "Employee";
        public const string Role_User_Company = "Company Customer";
        public const string Role_User_Individual = "Individual Customer";
        //Session
        public const string ssShoppingcartSession = "ShoppingCartSession";
        //orderStatus
        public const string StatusPending = "Pending";
        public const string StatusApproved = "Approved";
        public const string StatusProgress = "Processing";
        public const string StatusShipped = "Shipped";
        public const string StatuCancelled = "Cancelled";
        public const string StatusRefunded = "Refunded";
        //PaymentStatus
        public const string PaymentStatusPending = "Pending";
        public const string PaymentStatusApproved = "Approved";
        public const string PaymentStatusDelayPayment = "DelayPayment";
        public const string PaymentStatusRejected = "Rejected";

        //method to display the price 
        public static double GetPriceBasedOnQuantity(double quantity,double price,double price50,double price100)
        {
            if (quantity < 50)
                return price;
            else if (quantity < 100)
                return price50;
            else
                return price100;
        }
        //To remove HTML tag from Description 
        public static string ConvertToRawHtml(string Source)
        {
            char[] array = new char[Source.Length];
            int arrayIndex = 0;
            bool inside = false;
            for(int i=0;i<Source.Length; i++)
            {
                char Let = Source[i];
                if(Let=='<')
                {
                    inside = true;
                    continue; // continue statement will -start the loop from next step(next loop) and skip the upcomming code in the  running loop 
                }
                if(Let=='>')
                {
                    inside = false;
                    continue;
                }
                if(!inside)
                {
                    array[arrayIndex] = Let;
                    arrayIndex++;
                }
            }
            return new string (array, 0, arrayIndex);
        }



















    }
}
