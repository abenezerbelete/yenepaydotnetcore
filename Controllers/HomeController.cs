using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using YenePaySdk;

namespace yenepaydotnetcore.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly CheckoutOptions checkoutOptions;
    private string pdtToken  = "Q1woj27RY1EBsm";

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
        string sellerCode = "SB1151";
        string successUrlReturn = "http://localhost:5175/Home/PaymentSuccessReturnUrl"; //"YOUR_SUCCESS_URL";
        string ipnUrlReturn = "http://localhost:5175/Home/IPNDestination"; //"YOUR_IPN_URL";
        string cancelUrlReturn = "http://localhost:5175/Home/PaymentCancelReturnUrl"; //"YOUR_CANCEL_URL";
        string failureUrlReturn = ""; //"YOUR_FAILURE_URL";
        bool useSandBox = true;
        checkoutOptions = new CheckoutOptions(sellerCode, string.Empty, CheckoutType.Express, useSandBox, null, successUrlReturn, cancelUrlReturn, ipnUrlReturn, failureUrlReturn);
    }

    [HttpPost]
    public void CheckoutExpress()
    {
        checkoutOptions.Process = CheckoutType.Express;
        var itemId = Request.Form["ItemId"];
        var itemName = Request.Form["ItemName"];
        var unitPrice = decimal.Parse(Request.Form["UnitPrice"]);
        var quantity = int.Parse(Request.Form["Quantity"]);
        var discount = decimal.Parse(Request.Form["Discount"]);
        var deliveryFee = decimal.Parse(Request.Form["DeliveryFee"]);
        var handlingFee = decimal.Parse(Request.Form["HandlingFee"]);
        var tax1 = decimal.Parse(Request.Form["Tax1"]);
        var tax2 = decimal.Parse(Request.Form["Tax2"]);

        CheckoutItem checkoutitem = new CheckoutItem(itemId, itemName, unitPrice, quantity, tax1, tax2, discount, handlingFee, deliveryFee);
        checkoutOptions.OrderId = "12-34"; //"YOUR_UNIQUE_ID_FOR_THIS_ORDER";  //can also be set null
        checkoutOptions.ExpiresAfter = 2880; //"NUMBER_OF_MINUTES_BEFORE_THE_ORDER_EXPIRES"; //setting null means it never expires
        var url = CheckoutHelper.GetCheckoutUrl(checkoutOptions, checkoutitem);
        Response.Redirect(url);
    }

    [HttpGet]
    public IActionResult Cart()
    {
        return View();
    }

    [HttpPost]
    public ActionResult CheckoutCart([FromBody] List<CheckoutItem> Items)
    {
        checkoutOptions.Process = CheckoutType.Cart;
        decimal? totalItemsDeliveryFee = 10;
        decimal? totalItemsDiscount = 5;
        decimal? totalItemsHandlingFee = 6;
        decimal? totalItemsTax1 = Items.Sum(i => (i.UnitPrice * i.Quantity)) * (decimal)0.15;
        decimal? totalItemsTax2 = 0;
        checkoutOptions.SetOrderFees(totalItemsDeliveryFee, totalItemsDiscount, totalItemsHandlingFee, totalItemsTax1, totalItemsTax2);

        checkoutOptions.OrderId = "AB-CD"; //"YOUR_UNIQUE_ID_FOR_THIS_ORDER";  //can also be set null
        checkoutOptions.ExpiresAfter = 2880; //"NUMBER_OF_MINUTES_BEFORE_THE_ORDER_EXPIRES"; //setting null means it never expires

        var url = CheckoutHelper.GetCheckoutUrl(checkoutOptions, Items);
        return Json(new { redirectUrl = url });
    }

    [HttpPost]
    public async Task<string> IPNDestination(IPNModel ipnModel)
    {
        var result = string.Empty;
        ipnModel.UseSandbox = checkoutOptions.UseSandbox;
        if (ipnModel != null)
        {
            var isIPNValid = await CheckIPN(ipnModel);

            if (isIPNValid)
            {
                //This means the payment is completed
                //You can now mark the order as "Paid" or "Completed" here and start the delivery process
                Console.Write("Can Start Delivery");
            }
        }
        return result;
    }

    public async Task<ActionResult> PaymentSuccessReturnUrl(IPNModel ipnModel)
    {
        PDTRequestModel model = new PDTRequestModel(pdtToken, ipnModel.TransactionId, ipnModel.MerchantOrderId);
        model.UseSandbox = checkoutOptions.UseSandbox;
        var pdtResponse = await CheckoutHelper.RequestPDT(model);
        if (pdtResponse.Count() > 0)
        {
            if (pdtResponse["Status"] == "Paid")
            {
                Console.Write(pdtResponse["Status"]);
                //This means the payment is completed. 
                //You can extract more information of the transaction from the pdtResponse dictionary
                //You can now mark the order as "Paid" or "Completed" here and start the delivery process
            }
        }
        else
        {
            Console.Write(pdtResponse["Status"]);
            //This means the pdt request has failed.
            //possible reasons are 
            //1. the TransactionId is not valid
            //2. the PDT_Key is incorrect
        }
        return Redirect("/");
    }

    public async Task<string> PaymentCancelReturnUrl(IPNModel ipnModel)
    {
        PDTRequestModel model = new PDTRequestModel(pdtToken, ipnModel.TransactionId, ipnModel.MerchantOrderId);
        var pdtResponse = await CheckoutHelper.RequestPDT(model);
        if (pdtResponse.Count() > 0)
        {
            if (pdtResponse["Status"] == "Canceled")
            {
                Console.Write(pdtResponse["Status"]);
                //This means the payment is canceled. 
                //You can extract more information of the transaction from the pdtResponse dictionary
                //You can now mark the order as "Canceled" here.
            }
        }
        else
        {
            Console.Write(pdtResponse["Status"]);
            //This means the pdt request has failed.
            //possible reasons are 
            //1. the TransactionId is not valid
            //2. the PDT_Key is incorrect
        }
        return string.Empty;
    }

    private async Task<bool> CheckIPN(IPNModel model)
    {
        return await CheckoutHelper.IsIPNAuthentic(model);
    }

    public IActionResult Index()
    {
        return View();
    }
}
