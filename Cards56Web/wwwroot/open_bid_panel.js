"use strict"

class BidPanelOpenTrump
{
    constructor(main_div)
    {
        this.bid_panel = $('<div/>').addClass('open_bid_panel').appendTo($(main_div)[0]);
        // $(this.bid_panel)[0].innerHTML = "<button class='obidbtn'>testing text</button>";
        $(this.bid_panel)[0].innerHTML = this._getBidPanelMarkup();
    }

    setBidBtnClicked = (BidBtnClickEvent) =>
    {
    };

    show = (minBid) =>
    {
        $(this.bid_panel)[0].style.display = 'block';
    }

    hide = () =>
    {
        $(this.bid_panel)[0].style.display = 'none';
    }
    _getABidMarkup = (bidNo) =>
    {
        return "<button class='obidbtn obidbtn-bid' id='btn" + bidNo + "'>" + bidNo + "</button>";
    }

    _getBiddingRowMarkup = (bidNumbers) =>
    {
        let bidTable = "<table class='obidtable' class='obidpannelstyle'>";
        bidTable += "<tr>";
        bidNumbers.forEach(bidNo =>
        {
            bidTable += "<td class='obidstylecell'>" + this._getABidMarkup(bidNo) + "</td>";
        });

        bidTable += "</tr>";
        bidTable += "</table>";
        return bidTable;
    }

    _getBidPanelMarkup = () =>
    {
        let bidMarkup='';
        bidMarkup += this._getBiddingRowMarkup([1, 2, 3]);
        bidMarkup += this._getBiddingRowMarkup([4, 5, 6]);
        bidMarkup += this._getBiddingRowMarkup([7, 8, 9]);
        bidMarkup += this._getBiddingRowMarkup(['+', 0, 'Del']);
        return bidMarkup;
    }


}

