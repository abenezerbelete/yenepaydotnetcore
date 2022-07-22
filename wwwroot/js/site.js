// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
$(document).ready(function () {
    var counter = 0;
    var Items = new Array();
    $('.sub').click(function (e) {
        var element = $(this);
        var ItemName = element.parent().children("input[name=ItemName]").val();
        var ItemId = element.parent().children("input[name=ItemId]").val();
        var UnitPrice = element.parent().children("input[name=UnitPrice]").val();
        var Quantity = element.parent().children("input[name=Quantity]").val();
        counter++;
        $("img[id=index-para]").append().css("border", "3px solid red");
        $("p[id=index-p]").html("#Items: " + counter);
        var item = {
            'ItemId': ItemId,
            'ItemName': ItemName,
            'UnitPrice': UnitPrice,
            'Quantity': Quantity           
        };

        Items.push(item);
        console.log(Items);
        
    });
    
    $('#checkout').click(function () {
        $.ajax({
            type: "POST",
            dataType: "json",
            url: "CheckoutCart",
            contentType: "application/json",
            data: JSON.stringify(Items),
            success: function (data) {
                if (data) {
                    window.location = data.redirectUrl;
                }
            },
            error: function (xhr, status, text) {
               
            }

        });
    });
});