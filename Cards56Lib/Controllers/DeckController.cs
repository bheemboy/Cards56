using System;
using System.Collections.Generic;

namespace Cards56Lib
{
    internal class DeckController
    {
        TableType T {get;}
        private List<string> Deck;
        private readonly object _cardsLock = new object();
        internal DeckController(TableType tableType, List<string> deck)
        {
            T = tableType;
            Deck = deck;
        }
        public List<string>[] DealCards()
        {
            lock(_cardsLock)
            {
                ShuffleCards();

                // get the first c cards
                List<string>[] withdrawCards = new List<string>[T.MaxPlayers];
                
                for(int i=0; i<T.MaxPlayers; i++)
                {
                    withdrawCards[i] = Deck.GetRange(0, T.DeckSize/T.MaxPlayers);
                    withdrawCards[i].Sort(T.CompareCards);
                    Deck.RemoveRange(0, T.DeckSize/T.MaxPlayers); // Remove withdrawn cards
                }

                return withdrawCards;
            }
        }
        private void ShuffleCards()
        {
            int ShuffleCount = 3;
            if (Deck==null || Deck.Count != T.DeckSize)
            {
                Deck = new List<string>();
                T.Suits.ForEach(s => T.Ranks.ForEach(r => Deck.Add(s+r)));
                T.Suits.ForEach(s => T.Ranks.ForEach(r => Deck.Add(s+r)));
                ShuffleCount += 3;
            }

            Action<int, int> shuffle = (start, count) =>
            {
                List<string> cards = Deck.GetRange(start, count);
                Deck.RemoveRange(start, count);
                Deck.AddRange(cards);
            };

            while (true)
            {
                Random rnd = new Random();
                for (int i = 0; i < ShuffleCount; i++)
                {
                    // Move cards from top to bottom
                    shuffle(0, rnd.Next(T.DeckSize/4, T.DeckSize/2));

                    // Move cards from middle to bottom
                    int start = rnd.Next(T.DeckSize/2);
                    shuffle(start, rnd.Next(T.DeckSize/2, T.DeckSize) - start);
                }
                if (IsValidShuffle()) break;
            }

            return;
        }
        private bool IsValidShuffle()
        {
            // Confirm that 0..7, 8..15, 16..23, 24..31 in Deck are not of the same suit
            int CardsPerPlayer = T.DeckSize/T.MaxPlayers;
            for (int i = 0; i < T.MaxPlayers; i++)
            {
                bool IsValid = false; 
                for (int j = 0; j < CardsPerPlayer-1; j++)
                {
                    if (Deck[i*CardsPerPlayer+j][0] != Deck[i*CardsPerPlayer+j+1][0]) 
                    {
                        IsValid = true;
                        break;
                    }
                }
                if (!IsValid) return false; 
            }
            return true;
        }
        public void ReturnCards(IReadOnlyList<string> cards)
        {
            lock(_cardsLock)
            {
                if (cards != null)
                {
                    Deck.AddRange(cards);
                }
            }
        }
        public void ReturnCard(string card)
        {
            lock(_cardsLock)
            {
                if (card != null)
                {
                    Deck.Add(card);
                }
            }
        }
    }
}
