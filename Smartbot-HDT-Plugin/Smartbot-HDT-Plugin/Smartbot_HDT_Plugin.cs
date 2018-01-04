using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hearthstone_Deck_Tracker.Hearthstone;
using Hearthstone_Deck_Tracker.Hearthstone.Entities;
using Hearthstone_Deck_Tracker.Enums.Hearthstone;
using Hearthstone_Deck_Tracker.API;
using Hearthstone_Deck_Tracker.LogReader;
using Hearthstone_Deck_Tracker.Enums;
using Hearthstone_Deck_Tracker.Plugins;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Hearthstone_Deck_Tracker;
using System.Reflection;
using Hearthstone_Deck_Tracker.Utility.Toasts;
using System.ComponentModel;
using System.Windows;
using static Hearthstone_Deck_Tracker.Windows.MessageDialogs;
using System.Xml;
using System.IO;
using System.Xml.Linq;
using Hearthstone_Deck_Tracker.Utility.Logging;


namespace Smartbot_HDT_Plugin
{
    public class Smartbot_HDT
    {
        public static String smartBotPath;
        public static Boolean finishedOnLoad = false;
        private static List<String> cardsInOpponentsDeckThatDidNotStartThere = new List<String>();

        internal static void TurnStart(ActivePlayer player)
        {
            ExportCurrentGameState();
        }

        internal static void GameStart()
        {
            ExportCurrentGameState();
        }

        internal static void PlayerDraw(Card cardDrawn)
        {
            ExportCurrentGameState();
        }

        internal static void PlayerDiscardFromDeck(Card cardDiscarded)
        {
            ExportCurrentGameState();
        }

        internal static void PlayerMulligan(Card cardMulliganed)
        {
            ExportCurrentGameState();
        }

        internal static void CardIntoPlayerDeck(Card cardAdded)
        {
            ExportCurrentGameState();
        }

        internal static void CardCreatedInPlayerDeck(Card cardAdded)
        {
            ExportCurrentGameState();
        }

        internal static void CardAddedToOpponentsDeck(Card cardAdded)
        {
            cardsInOpponentsDeckThatDidNotStartThere.Add(cardAdded.Id);
            ExportCurrentGameState();
        }

        internal static void ExportCurrentGameState()
        {
            if (finishedOnLoad == true)
            { //ONLY export when OnLoad finished
              //--Get player deck
                List<Card> playerDeckCardList = Hearthstone_Deck_Tracker.Core.Game.Player.PlayerCardList;
                List<String> playerDeck = new List<String>();
                foreach (Card x in playerDeckCardList)
                {
                    for (int c = 0; c < x.Count; c++)
                    {
                        playerDeck.Add(x.Id);
                    }
                }
                //--\\

                //--Get opponent deck
                List<Card> opponentDeckCardList = Hearthstone_Deck_Tracker.Core.Game.Opponent.OpponentCardList;
                List<String> opponentDeckList = new List<String>();
                foreach (Card x in opponentDeckCardList)
                {
                    for (int c = 0; c < x.Count; c++)
                    {
                        opponentDeckList.Add(x.Id);
                    }
                }
                foreach (String s in cardsInOpponentsDeckThatDidNotStartThere)
                {
                    opponentDeckList.Remove(s);
                }
                //--\\

                //Get opponent hand
                IEnumerable<Entity> opponnentCardsInHandEntities = Hearthstone_Deck_Tracker.Core.Game.Opponent.Hand;
                List<String> opponentHand = new List<String>();

                foreach (Entity x in opponnentCardsInHandEntities)
                {
                    opponentHand.Add(x.CardId);
                }
                //--\\

                //--Put together a JSON Object
                JObject json = new JObject();
                json.Add("playersDeck", JsonConvert.SerializeObject(playerDeck));
                json.Add("currentOpponentHand", JsonConvert.SerializeObject(opponentHand));
                json.Add("opponentsDeckList", JsonConvert.SerializeObject(opponentDeckList));
                //--\\


                //if HDT folder does not exist, create it:
                String directoryPath = Path.Combine(smartBotPath, "HDT");
                Directory.CreateDirectory(directoryPath);

                //--Export the JSON to a file
                String jsonPath = Path.Combine(directoryPath, "data.json");
                System.IO.File.WriteAllText(jsonPath, string.Empty); //Empty the file
                System.IO.File.WriteAllText(jsonPath, JsonConvert.SerializeObject(json)); //Write the json object into the json file.
                                                                                          //--\\
            }
        }
    }

    public class Smartbot_HDT_Plugin : IPlugin
    {

        public void OnLoad()
        {

            //--All the GameEvents 
            GameEvents.OnGameStart.Add(Smartbot_HDT.GameStart);
            GameEvents.OnTurnStart.Add(Smartbot_HDT.TurnStart);
            GameEvents.OnPlayerDraw.Add(Smartbot_HDT.PlayerDraw);
            GameEvents.OnPlayerDeckDiscard.Add(Smartbot_HDT.PlayerDiscardFromDeck);
            GameEvents.OnPlayerMulligan.Add(Smartbot_HDT.PlayerMulligan);
            GameEvents.OnPlayerPlayToDeck.Add(Smartbot_HDT.CardIntoPlayerDeck);
            GameEvents.OnPlayerCreateInDeck.Add(Smartbot_HDT.CardCreatedInPlayerDeck);
            GameEvents.OnOpponentCreateInDeck.Add(Smartbot_HDT.CardAddedToOpponentsDeck);
            GameEvents.OnOpponentPlayToDeck.Add(Smartbot_HDT.CardAddedToOpponentsDeck);
            //--\\

            //--Reads settings File
            string settingsFolder = Assembly.GetExecutingAssembly().Location.Replace(@"\Smartbot-HDT-Plugin.dll", ""); //Getting .dll (plugin) location
            string settingsPath = Path.Combine(settingsFolder, "Settings.xml");
            Log.WriteLine("Setting path is: " + settingsPath, LogType.Info);
            string xmlFileString = File.ReadAllText(settingsPath); //Reading XML to string

            //Removing byteOrderMark if needed
            string _byteOrderMarkUtf8 = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble());
            if (xmlFileString.StartsWith(_byteOrderMarkUtf8))
            {
                var lastIndexOfUtf8 = _byteOrderMarkUtf8.Length - 1;
                xmlFileString = xmlFileString.Remove(0, lastIndexOfUtf8);
            }


            XmlDocument xml = new XmlDocument();
            xml.LoadXml(xmlFileString);



            //Getting setting with the type "path"
            XmlNode pathNode = xml.SelectSingleNode("/Settings/setting[@type='path']");
            Smartbot_HDT.smartBotPath = pathNode.InnerText;

            //--\\
            Smartbot_HDT.finishedOnLoad = true; ///Starts exporting
        }

        public void OnUnload()
        {

        }

        public void OnButtonPress()
        {
        }

        public void OnUpdate() // called every ~100ms
        {
            Smartbot_HDT.ExportCurrentGameState();

        }

        public static void saveSettings()
        {

        }

        public static void loadSettings()
        {

        }


        public string Name => "SB Plugin";

        public string Description => "Exports information from HDT to a JSON file for SB to use." + Environment.NewLine + "Change SB Path in the settings.ini in the plugin folder!";

        public string ButtonText => "KryTon sucks!";

        public string Author => "KryTonX";

        public Version Version => new Version(0, 0, 5);

        public MenuItem MenuItem => null;
    }
}
