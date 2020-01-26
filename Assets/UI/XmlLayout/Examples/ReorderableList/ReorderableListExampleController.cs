using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UI.Xml;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace UI.Xml.Examples
{
    class ReorderableListExampleController : XmlLayoutController
    {
        private XmlElementReference<XmlElement> songList;
        private XmlElementReference<XmlElement> songTemplate;
        private XmlElementReference<XmlElement> nowPlayingText;
        private XmlElementReference<XmlElement> bottom;

        public List<Song> playList = new List<Song>();

        private Song currentSong;

        void Start()
        {
            songList = XmlElementReference<XmlElement>("songList");
            songTemplate = XmlElementReference<XmlElement>("songTemplate");
            nowPlayingText = XmlElementReference<XmlElement>("nowPlayingText");
            bottom = XmlElementReference<XmlElement>("bottom");

            // Add sample data if the list hasn't been populated in the inspector beforehand
            if (playList.Count == 0)
            {
                playList.Add(new Song("Song 1", "Artist 1", "Songs/1.mp3"));
                playList.Add(new Song("Song 2", "Artist 2", "Songs/2.mp3"));
                playList.Add(new Song("Song 3", "Artist 3", "Songs/3.mp3"));
                playList.Add(new Song("Song 4", "Artist 4", "Songs/4.mp3"));
                playList.Add(new Song("Song 5", "Artist 5", "Songs/5.mp3"));
            }

            foreach (var song in playList)
            {
                AddSong(song);
            }
        }

        void AddSong(Song song)
        {
            var newElement = Instantiate(songTemplate.element);
            newElement.SetAttribute("active", "true");
            newElement.SetAttribute("internalId", song.GetHashCode().ToString());
            newElement.ApplyAttributes();

            newElement.GetElementByInternalId("songName").SetAndApplyAttribute("text", song.name);
            newElement.GetElementByInternalId("songArtist").SetAndApplyAttribute("text", song.artist);

            newElement.GetElementByInternalId("songPlayButton").SetAndApplyAttribute("onClick", "PlaySong(" + song.name + ")");
            newElement.GetElementByInternalId("songRemoveButton").SetAndApplyAttribute("onClick", "RemoveSong(" + song.name + ")");

            songList.element.AddChildElement(newElement);

            // The bottom element must always be last
            bottom.element.transform.SetAsLastSibling();
        }

        void SongDropped(XmlElement songElement, XmlElement droppedOn)
        {
            if (droppedOn.HasClass("song"))
            {
                // we've dropped on a song, take its place
                songElement.transform.SetSiblingIndex(droppedOn.transform.GetSiblingIndex());
            }
            else if (droppedOn.HasClass("header"))
            {
                // we've dropped on the header, move to the top of the list
                songElement.transform.SetAsFirstSibling();
            }
            else if (droppedOn.HasClass("bottom"))
            {
                // we've dropped on the song list (which can only happen at the end of the list)
                songElement.transform.SetAsLastSibling();
            }
            else
            {
                // the remaining possibility is that the element was dropped back in its initial position; do nothing
                return;
            }

            // The bottom element must always be last
            bottom.element.transform.SetAsLastSibling();

            // get the song name from the text element
            var songName = songElement.GetElementByInternalId<Text>("songName").text;

            // now use our new sibling index to determine where in the data collection the item should be moved
            var song = playList.FirstOrDefault(s => s.name == songName);

            if (song != null)
            {
                var oldIndex = playList.IndexOf(song);
                playList.RemoveAt(oldIndex);

                var newIndex = songElement.transform.GetSiblingIndex();
                if (newIndex > oldIndex) newIndex--;

                playList.Insert(newIndex, song);
            }
        }

        void PlaySong(string name)
        {
            // Remove the current highlight on any songs
            var elementsToClear = songList.element.GetChildElementsWithClass("currentSong");
            foreach (var elementToClear in elementsToClear)
            {
                elementToClear.RemoveClass("currentSong");
            }

            var song = playList.FirstOrDefault(s => s.name == name);

            if (song != null)
            {
                currentSong = song;
                nowPlayingText.element.SetAndApplyAttribute("Text", "'" + song.name + "' by '" + song.artist + "'.");

                // highlight the currently playing song
                var element = songList.element.GetElementByInternalId(song.GetHashCode().ToString());
                element.AddClass("currentSong");
            }

            // Here's where you would add code to actually play the song
        }

        void PlayFirst()
        {
            PlaySong(playList.First().name);
        }

        void PlayLast()
        {
            PlaySong(playList.Last().name);
        }

        void PlayNext()
        {
            if (currentSong == null)
            {
                PlayFirst();
            }
            else
            {
                var index = playList.IndexOf(currentSong);

                if (index == playList.Count - 1)
                {
                    PlayFirst();
                }
                else
                {
                    var nextSong = playList[index + 1];
                    PlaySong(nextSong.name);
                }
            }
        }

        void PlayPrevious()
        {
            if (currentSong == null)
            {
                PlayLast();
            }
            else
            {
                var index = playList.IndexOf(currentSong);

                if (index == 0)
                {
                    PlayLast();
                }
                else
                {
                    var previousSong = playList[index - 1];
                    PlaySong(previousSong.name);
                }
            }
        }

        void RemoveSong(string name)
        {
            var song = playList.FirstOrDefault(s => s.name == name);

            if (song != null)
            {
                // remove the element
                var element = songList.element.GetElementByInternalId(song.GetHashCode().ToString());
                if (element != null) Destroy(element.gameObject);

                // remove the song from the playlist collection
                playList.Remove(song);
            }
        }
    }

    [Serializable]
    class Song
    {
        public Song(string name, string artist, string filePath)
        {
            this.name = name;
            this.artist = artist;
            this.filePath = filePath;
        }

        public string name = null;
        public string artist = null;
        public string filePath = null;
    }
}
