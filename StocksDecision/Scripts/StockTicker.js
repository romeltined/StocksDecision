// A simple templating method for replacing placeholders enclosed in curly braces.
if (!String.prototype.supplant) {
    String.prototype.supplant = function (o) {
        return this.replace(/{([^{}]*)}/g,
            function (a, b) {
                var r = o[b];
                return typeof r === 'string' || typeof r === 'number' ? r : a;
            }
        );
    };
}

$(function () {

    var ticker = $.connection.stockTickerMini, // the generated client-side hub proxy
        $stockTable = $('#stockTable'),
        $stockTableBody = $stockTable.find('tbody'),
        rowTemplate = '<tr data-value="{Symbol}"> \
                       <td style="display:none">{Symbol}</td> \
                       <td><span class="fa fa-line-chart" style="font-size:12px;color:green"></span> <a href="/Stocks/FilterIndex?symbol={Symbol}" data-toggle="tooltip" title="Trend">{Symbol}</a></td> \
                       <td id={Symbol}Quantity>{Quantity}</td> \
                       <td class="{LastPriceColor}">{_LastPrice}</td> \
                       <td >{CurrentValue}</td> \
                       <td class="{GainLossColor}" >{GainLoss}</td> \
                       <td id={Symbol}Buy>{_BuyTarget}</td> \
                       <td class="{BuyDecisionColor}">{BuyDecision}</td> \
                       <td id={Symbol}Sell>{_SellTarget}</td> \
                       <td class="{SellDecisionColor}">{SellDecision}</td> \
                       <td><a href="/StockMarkets/Edit/{Id}" data-toggle="tooltip" title="Edit"><span class="glyphicon glyphicon-edit"></span></a>  | \
                       <a href="/StockMarkets/Delete/{Id}" data-toggle="tooltip" title="Delete"><span class="glyphicon glyphicon-trash"></span></a> | \
                       <a href="/Stocks/Index?searchString={Symbol}" data-toggle="tooltip" title="Data"><span class="glyphicon glyphicon-list-alt"></span></a></td> \
                       <td id={Symbol}Id style="display:none;">{Id}</td>\
                       </tr>';

    function formatStock(stock) {

        // Create our number formatter.
        var formatter = new Intl.NumberFormat('en-US', {
            style: 'currency',
            currency: 'USD',
            minimumFractionDigits: 2,
            // the default value for minimumFractionDigits depends on the currency
            // and is usually already 2
        });
                //alert($(me).text());
        // $(stock.Symbol + 'Quantity').val();
        var _Id = '#' + stock.Symbol + 'Id',
            _Qty = '#' + stock.Symbol + 'Quantity',
            _Buy = '#' + stock.Symbol + 'Buy',
            _Sell = '#' + stock.Symbol + 'Sell',

            _id = $(_Id).text().trim(),
            _qty = parseFloat($(_Qty).text()).toFixed(2),
            _buy = parseFloat($(_Buy).text().replace('$', '')),
            _sell = parseFloat($(_Sell).text().replace('$', '')),
            tolerance = 0.3; 

        return $.extend(stock, {
            Id: _id,
            Symbol: stock.Symbol,
            Quantity: _qty, //(stock.Quantity == null) ? '' : stock.Quantity,
            CurrentValue: (_qty == null) ? '' : formatter.format(_qty * stock.LastPrice),
            _LastPrice: formatter.format(stock.LastPrice),
            _BuyTarget: (_buy == null) ? '' : formatter.format(_buy),
            BuyDecision: (stock.LastPrice <= (_buy + tolerance) && _buy > 0) ? 'Buy' : 'Hold',
            _SellTarget: (_sell == null) ? '' : formatter.format(_sell),
            SellDecision: (stock.LastPrice >= (_sell - tolerance) && _sell>0) ? 'Sell' : 'Hold',
            LastPriceColor: (stock.LastPrice >= stock.PreviousPrice) ? 'success' : 'danger',
            BuyDecisionColor: (stock.LastPrice <= (_buy + tolerance) && _buy > 0) ? 'success' : '',
            SellDecisionColor: (stock.LastPrice >= (_sell - tolerance) && _sell > 0) ? 'success' : '',
            GainLoss: (_buy == null) ? '' : formatter.format(_qty * (stock.LastPrice - _buy)),
            GainLossColor: (stock.LastPrice > _buy && _qty!=0) ? 'success' : (_qty==0)?'':'danger'
        });


    }
    //getAllStocks
    function init() {
        ticker.server.getAllStocks().done(function (stocks) {
            $.each(stocks, function () {
                refreshRows(this);
            });
            $("#stockTable").show();
            $("#loading").hide();
        });
    }

    function refreshRows(_stock) {
        var stock = formatStock(_stock);
        $row = $(rowTemplate.supplant(stock));

        var exist = $stockTableBody.find('tr[data-value=' +   stock.Symbol + ']');
        if (exist.length > 0) {
            $stockTableBody.find('tr[data-value='    + stock.Symbol + ']')
                .replaceWith($row);
        }
 
    }

        ticker.client.updateAllStockPrice = function (stocks) {
            $.each(stocks, function () {
                refreshRows(this);
            });

            var result = getStock(stocks, 'ACN'); //stocks.filter(function (obj) { return obj.Symbol == 'AMD'; });
            alert(result.LastPrice);
           // loopTable(stocks);
        }

        function getStock(stocks, symbol)
        {
            $.each(stocks, function () {
                if (this.Symbol == symbol)
                {
                    return this;
                }
            });
        }
        
        function loopTable(stocks) {
            var table = document.getElementById("stockMarket");
            for (var i = 0, row; row = table.rows[i]; i++) {
                //iterate through rows
                //rows would be accessed using the "row" variable assigned in the for loop
                for (var j = 0, col; col = row.cells[j]; j++) {
                    //iterate through columns
                    //columns would be accessed using the "col" variable assigned in the for loop
                    var result = col.innerHTML; //table.rows[i].cells[j].innerHTML;
                    if (j == 0) {
                        var result = stocks.filter(function (obj) { return obj.Symbol == result; });
                    
                        //alert(result.LastPrice);
                    }
                    //col.innerHTML = 'memel';

                }  
            }
            
        }


    // Start the connection
    $.connection.hub.start().done(init);
    //$.connection.hub.start().done();
});