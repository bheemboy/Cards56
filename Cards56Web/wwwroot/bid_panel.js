"use strict"

class BidPanel
{
    constructor(main_div)
    {
        this.bid_panel = $('<div/>').addClass('bid_panel').appendTo($(main_div)[0]);
        $(this.bid_panel)[0].innerHTML = this._getBidPanelMarkup();
        $(".bidbtn").click(this._onBtnClick);
    }

    _bidEvent = (bid) =>
    {
        alert(bid);
    }

    setBidBtnClicked = (BidBtnClickEvent) =>
    {
        this._bidEvent = BidBtnClickEvent;
    };
    _getABidMarkup = (bidNo) =>
    {
        if (bidNo == 57)
        {
            return "<button class='bidbtn bidbtn-thani' id='btn57'>Thani</button>";
        } else if (bidNo == 'P')
        {
            return "<button class='bidbtn bidbtn-pass' id='btn0'>Pass</button>";
        }
        else
        {
            return "<button class='bidbtn bidbtn-bid' id='btn" + bidNo + "'>" + bidNo + "</button>";
        }
    }

    _getBiddingRowMarkup = (bidNumbers) =>
    {
        let bidTable = "<table class='bidtable' class='bidpannelstyle'>";
        bidTable += "<tr>";
        bidNumbers.forEach(bidNo =>
        {
            bidTable += "<td class='bidstylecell'>" + this._getABidMarkup(bidNo) + "</td>";
        });

        bidTable += "</tr>";
        bidTable += "</table>";
        return bidTable;
    }

    _getBidPanelMarkup = () =>
    {
        let bidMarkup='';
        bidMarkup += this._getBiddingRowMarkup([48, 49, 50, 51, 52, 53, 54, 55, 56, 57]);
        bidMarkup += this._getBiddingRowMarkup([40, 41, 42, 43, 44, 45, 46, 47]);
        bidMarkup += this._getBiddingRowMarkup([28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39]);
        bidMarkup += this._getBiddingRowMarkup(['P']);

        return bidMarkup;
    }

    _onBtnClick = (e) =>
    {
        let biddingFor = e.currentTarget.id.substring(3);
        let bid = parseInt(biddingFor);
        this._bidEvent(bid);
    }

    show = (minBid) =>
    {
        for (let i=28; i<=57; i++)
        {
            $("#btn"+i).prop('disabled', (i<minBid));
            if (i<minBid)
            {
                $("#btn"+i).addClass('bidbtn-disable');
            }
            else
            {
                $("#btn"+i).removeClass('bidbtn-disable');
            }
        }
        $(this.bid_panel)[0].style.display = 'block';
    }

    hide = () =>
    {
        $(this.bid_panel)[0].style.display = 'none';
    }
}

