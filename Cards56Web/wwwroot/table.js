"use strict"
class Table
{
  constructor(main_div, player_count)
  {
    this._main_div = main_div;
    this.player_count = player_count;
    
    cards.init({ table: this._main_div, type: PINOCHLE });

    this._init_player_hand();
    this._init_last_round();
    this._init_rounds();
    this._init_trump_card();
    this._init_koolies();
    this._init_scores();
    this._init_center_display();
    this._init_version("2022.04.15");
    this._init_alert_message();
    this._init_table_buttons();
  }

  _init_player_hand = () =>
  {
    this.player_hand = new cards.Hand({ faceUp: true, y: 570 });
  }

  _init_last_round = () =>
  {
    this.last_round = new cards.Hand({ faceUp: true, x: 125, y: 635 });
  }

  _init_rounds = () =>
  {
    let round_posns_4 = [{ x: 0, y: 100 }, { x: 125, y: -25 }, { x: 0, y: -150 }, { x: -125, y: -25 },];
    let round_posns_6 = [{ x: 0, y: 100 }, { x: 125, y: 45 }, { x: 125, y: -85 }, { x: 0, y: -150 }, { x: -125, y: -85 }, { x: -125, y: 45 },];
    let round_posns_8 = [{ x: 0, y: 115 }, { x: 110, y: 90 }, { x: 155, y: -25 }, { x: 110, y: -140 }, 
                         { x: 0, y: -160 }, { x: -110, y: -140 }, { x: -155, y: -25 }, { x: -110, y: 90 },];
    let round_posn = round_posns_4;
    if (this.player_count == 6)
    {
      round_posn = round_posns_6;
    }
    else if (this.player_count == 8)
    {
      round_posn = round_posns_8;
    }

    this.rounds = [];
    for (let i = 0; i < this.player_count; i++)
    {
      let round = new cards.Deck({ faceUp: true });
      this.rounds.push(round);
      round.x += round_posn[i].x;
      round.y += round_posn[i].y;
    }
  }

  _init_trump_card = () =>
  {
    this.trump_card = new cards.Deck({ faceUp: false });
    this.trump_card.x += 280;
    this.trump_card.y += 250;
  }

  _init_koolies = () =>
  {
    let koolies_div_my_team = $('<div/>').addClass('koolies myteam').appendTo($(this._main_div)[0]);
    let koolies_div_other_team = $('<div/>').addClass('koolies otherteam').appendTo($(this._main_div)[0]);
    this.koolies_my_team = [];
    this.koolies_other_team = [];

    for (let i = 0; i < 14; i++)
    {
      this.koolies_my_team.push($('<div/>').addClass('koolie myteam').appendTo($(koolies_div_my_team)[0]));
      this.koolies_other_team.push($('<div/>').addClass('koolie otherteam').appendTo($(koolies_div_other_team)[0]));
    }
  }

  _init_scores = () =>
  {
    this._team_score = $('<div/>').addClass('score score-myteam').appendTo($(this._main_div)[0]);
    this._opponent_score = $('<div/>').addClass('score score-otherteam').appendTo($(this._main_div)[0]);
  }

  _init_center_display = () =>
  {
    this._center_display = $('<div/>').addClass('center-display').appendTo($(this._main_div)[0]);
  }

  _init_alert_message = () =>
  {
    this._alert_div = $('<div/>').addClass('alert').appendTo($(this._main_div)[0]);
    this._alert_msg = $('<div/>').addClass('alert-msg').appendTo($(this._alert_div)[0]);
    this._alert_span = $('<span>&times;</span>').addClass('closebtn').appendTo($(this._alert_div)[0]);

    $(this._alert_span).click(function ()
    {
      let div = this.parentElement;
      div.style.opacity = "0";
      setTimeout(function () { div.style.display = "none"; }, 600);
    });
  }

  _init_version = (ver) =>
  {
    this._init_version_label = $('<div/>').addClass('version-label-div').appendTo($(this._main_div)[0]);
    $(this._init_version_label)[0].innerHTML = "<A class='version-label' href='https://github.com/bheemboy/Cards56/releases/tag/" + ver + "' target='_blank' rel='noopener'>Version: " + ver + "</A>";
  }

  _init_table_buttons = () =>
  {
    this._table_buttons_div = $('<div/>').addClass('table-buttons').appendTo($(this._main_div)[0]);

    this._logout_button = $('<button/>').addClass('table-button logout').appendTo($(this._table_buttons_div)[0]);
    $(this._logout_button)[0].textContent = "Logout";
    $(this._logout_button).click(function () { location.replace("./"); });

    this._forfeit_button = $('<button/>').addClass('table-button forfeit').appendTo($(this._table_buttons_div)[0]);
    $(this._forfeit_button)[0].textContent = "Forfeit";

    this._new_game_button = $('<button/>').addClass('table-button new_game hidden').appendTo($(this._table_buttons_div)[0]);
    $(this._new_game_button)[0].textContent = "New Game";
  }

  ShowAlertMessage = (val, timeout) =>
  {
    let self = this;
    if (val)
    {
      $(this._alert_msg)[0].textContent = val;
      $(this._alert_div)[0].style.display = 'block';
      $(this._alert_div)[0].style.opacity = "1";
      if (timeout > 0)
      {
        setTimeout(function () { $(self._alert_span).trigger('click'); }, timeout);
      }
    }
    else
    {
      $(self._alert_span).trigger('click');
    }
  }

  ShowCenterMessage = (val) =>
  {
    if (val)
    {
      $(this._center_display).removeClass('hidden');
    }
    else
    {
      $(this._center_display).addClass('hidden');
    }

    $(this._center_display)[0].textContent = val;
  }

  ShowGameOverButton = (val) =>
  {
    $(this._forfeit_button).prop('disabled', val);
    if (val)
    {
      $(this._new_game_button).removeClass('hidden');
      $(this._forfeit_button).addClass('disabled');
    }
    else
    {
      $(this._new_game_button).addClass('hidden');
      $(this._forfeit_button).removeClass('disabled');
    }
  }
  SetPlayerCardClicked = (PlayerCardClickedEvent) =>
  {
    this.player_hand.click(PlayerCardClickedEvent);
  }

  SetTrumpCardClicked = (TrumpCardClickedEvent) =>
  {
    this.trump_card.click(TrumpCardClickedEvent);
  }

  SetNewGameClicked = (NewGameClickedEvent) =>
  {
    this._new_game_button.click(NewGameClickedEvent);
  }

  SetForfeitClicked = (ForfeitClickedEvent) =>
  {
    $(this._forfeit_button).click(ForfeitClickedEvent);
  }

  _get_card_obj = (player_card) =>
  {
    if (player_card.substr(1) == "1")
    {
      player_card = player_card.substr(0, 1) + "14";
    }
    let card_objs = cards.all.filter(
      (obj) => obj.shortName == player_card && obj.container == null
    );
    if (card_objs.length > 0)
    {
      return card_objs[0];
    }
    alert("Could not find unused '" + player_card + "' in library of cards.");
    return null;
  }

  _ShowDeckCard = (deck, str_card) =>
  {
    while (deck.length > 0)
    {
      let card = deck.topCard();
      card.container = null;
      deck.removeCard(card);
    }
    if (str_card)
    {
      deck.addCard(this._get_card_obj(str_card));
    }
    deck.render({ immediate: true });
  }

  ShowRoundCards = (is_thani, start_pons, round_cards) =>
  {
    for (let i = 0; i < this.player_count; i++)
    {
      let deck_posn = (i + start_pons) % this.player_count;
      let deck = this.rounds[deck_posn];
      let card = null;

      if (is_thani)
      {
        if (i == 0)
        {
          // first player == bidder plays the first card
          if (round_cards.length > 0) { card = round_cards[i]; }
        }
        else
        {
          // afer that only the odd decks get a card
          if ((i % 2) == 1)
          {
            // ith card belongs to deck [i*2 - 1]
            let card_posn = (i + 1) / 2;
            if (round_cards.length > card_posn) { card = round_cards[card_posn]; }
          }
        }
      }
      else
      {
        if (round_cards.length > i) { card = round_cards[i]; }
      }

      this._ShowDeckCard(deck, card);
    }
  }

  _ShowHandCards(hand, player_cards)
  {
    while (hand.length > 0)
    {
      let card = hand.topCard();
      card.container = null;
      hand.removeCard(card);
    }

    if (player_cards)
    {
      for (let i = 0; i < player_cards.length; i++)
      {
        hand.addCard(this._get_card_obj(player_cards[i]));
      }
    }

    hand.render({ immediate: true });
  }

  ShowPlayerCards = (player_cards) =>
  {
    this._ShowHandCards(this.player_hand, player_cards);
  }

  ShowLastRoundCards = (round_cards) =>
  {
    this._ShowHandCards(this.last_round, round_cards);
  }

  ShowTrump = (show, open, trump_card) =>
  {
    while (this.trump_card.length > 0)
    {
      let card = this.trump_card.topCard();
      card.container = null;
      this.trump_card.removeCard(card);
    }
    if (show)
    {
      if (trump_card)
      {
        this.trump_card.addCard(this._get_card_obj(trump_card));
      }
      else
      {
        this.trump_card.addCard(this._get_card_obj('c1'));
      }
      this.trump_card.faceUp = open;
    }
    this.trump_card.render({ immediate: true });
  }

  SetKoolies = (myteam, myteam_koolies, otherteam_koolies) =>
  {
    for (let i = 0; i < this.koolies_my_team.length; i++)
    {
      $(this.koolies_my_team[i]).removeClass('team' + ((myteam + 1) % 2)).addClass('team' + (myteam % 2));
      if (i < myteam_koolies)
      {
        $(this.koolies_my_team[i]).removeClass('hidden');
      }
      else
      {
        $(this.koolies_my_team[i]).addClass('hidden');
      }
    }

    for (let i = 0; i < this.koolies_other_team.length; i++)
    {
      $(this.koolies_other_team[i]).removeClass('team' + (myteam % 2)).addClass('team' + ((myteam + 1) % 2));
      if (i < otherteam_koolies)
      {
        $(this.koolies_other_team[i]).removeClass('hidden');
      }
      else
      {
        $(this.koolies_other_team[i]).addClass('hidden');
      }
    }
  }

  ShowScores = (myteam, str_my_team_score, str_opponent_score) =>
  {
    $(this._team_score)[0].textContent = str_my_team_score;
    $(this._opponent_score)[0].textContent = str_opponent_score;

    $(this._team_score).removeClass('team' + ((myteam + 1) % 2)).addClass('team' + (myteam % 2));
    $(this._opponent_score).removeClass('team' + ((myteam) % 2)).addClass('team' + ((myteam + 1) % 2));
  }
}
