"use strict"

class Player
{
  constructor(main_div, table_type, posn)
  {
    this._table_type = table_type;
    this._posn = posn;
    this._main_div = main_div;

    this._init_player_div();
  }

  _init_player_div = () =>
  {
    let posn_class_4 = [' bottom', ' other right', ' other top', ' other left'];
    let posn_class_6 = [' bottom', ' other right lower', ' other right upper', ' other top', ' other left upper', ' other left lower'];
    let posn_class_8 = [' bottom', ' other right lower', ' other right', ' other right upper', ' other top', ' other left upper', ' other left', ' other left lower'];
    let add_cls = "";
    if (this._table_type == 0)
    {
      add_cls = " 4" + posn_class_4[this._posn];
    }
    else if (this._table_type == 1 || this._table_type == 3)
    {
      add_cls = " 6" + posn_class_6[this._posn];
    }
    else
    {
      add_cls = " 8" + posn_class_8[this._posn];
    }

    if (this._posn != 0) add_cls += ' other';

    this._player_div = $('<div/>').addClass('player' + add_cls).appendTo($(this._main_div)[0]);
    this._dealer_div = $('<div/>').addClass('dealer' + add_cls).appendTo($(this._player_div)[0]);
    this._bid_div = $('<div/>').addClass('bid' + add_cls).appendTo($(this._player_div)[0]);
    this._player_icon_div = $('<div/>').addClass('player-icon' + add_cls).appendTo($(this._player_div)[0]);
    this._player_name_div = $('<div/>').addClass('player-name' + add_cls).appendTo($(this._player_icon_div)[0]);

    let _kunuk_box = $('<div/>').addClass('kunuk-box' + add_cls).appendTo($(this._player_div)[0]);
    this._kunuk_divs = [];
    for (let i = 0; i < 14; i++)
    {
      let kunuk_div = $('<div/>').addClass('kunuk' + add_cls).appendTo($(_kunuk_box)[0]);
      this._kunuk_divs.push(kunuk_div);
    }
    this._kunuk_just_installed = $('<div/>').addClass('kunuk-just-installed' + add_cls).appendTo($(this._player_div)[0]);

    let _watchers_box = $('<div/>').addClass('watcher-box' + add_cls).appendTo($(this._player_div)[0]);
    this._watcher_divs = [];
    for (let i = 0; i < 2; i++)
    {
      let watcher_div = $('<div/>').addClass('watcher' + add_cls).appendTo($(_watchers_box)[0]);
      this._watcher_divs.push(watcher_div);
    }
  }

  set_watchers(watchers)
  {
    for (let i=0; i<this._watcher_divs.length; i++)
    {
      if (i < watchers.length)
      {
        $(this._watcher_divs[i])[0].textContent = watchers[i].Name;
        $(this._watcher_divs[i]).addClass('visible');
      }
      else
      {
        $(this._watcher_divs[i])[0].textContent = '';
        $(this._watcher_divs[i]).removeClass('visible');
      }
    }
  }

  set_name(val)
  {
    $(this._player_name_div)[0].textContent = val;
  }

  set_dealer(val)
  {
    if (val)
    {
      $(this._dealer_div).addClass('visible');
    }
    else
    {
      $(this._dealer_div).removeClass('visible');
    }
  }

  set_team(val)
  {
    $(this._player_icon_div).removeClass('team' + ((val + 1) % 2));
    $(this._player_icon_div).addClass('team' + (val % 2));
  }

  clear_bid()
  {
    $(this._bid_div).removeClass('high_bid').removeClass('current_bid').removeClass('previous_bid');
  }

  set_high_bid(val)
  {
    if (val>0)
    {
      $(this._bid_div)[0].textContent = val;
      $(this._bid_div).addClass('high_bid');
    }
    else
    {
      $(this._bid_div)[0].textContent = '';
      $(this._bid_div).removeClass('high_bid');
    }
  }

  set_current_bid(val)
  {
    $(this._bid_div)[0].textContent = (val==0) ? '' : val;
    $(this._bid_div).addClass('current_bid');
  }

  set_previous_bid(val)
  {
    $(this._bid_div)[0].textContent = (val==0) ? '' : val;
    $(this._bid_div).addClass('previous_bid');
  }

  set_focus(val)
  {
    if (val)
    {
      $(this._player_icon_div).addClass('focus');
    }
    else
    {
      $(this._player_icon_div).removeClass('focus');
    }
  }

  show_kodies(kodi_count, kunuk_just_installed)
  {
    for (let i = 0; i < this._kunuk_divs.length; i++)
    {
      if (i < kodi_count)
      {
        $(this._kunuk_divs[i]).addClass('visible');
      }
      else
      {
        $(this._kunuk_divs[i]).removeClass('visible');
      }
    }
    if (kunuk_just_installed)
    {
      $(this._kunuk_just_installed).addClass('visible');
      let _kunuk_just_installed = this._kunuk_just_installed;
      // setTimeout(
      //   function () 
      //   { 
      //     $(_kunuk_just_installed).removeClass('visible');
      //   }, 4000
      // );
    }
    else
    {
      $(this._kunuk_just_installed).removeClass('visible');
    }
    
  }
}
