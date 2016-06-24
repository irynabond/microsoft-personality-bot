
using com.traitify.net.TraitifyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraitifyAPI
{

    public class TestType
    {
        Traitify traitify = new Traitify("https://api.traitify.com", "bt5bo444mk38efl4crbat39scv", "pl17doqlrachpa7jg5oqe3ja7p", "v1");

        static private string assessment_id;

        public List<string> GetNames()
        {
            List<string> deckNames = new List<string>();
            List<Deck> decks = traitify.GetDecks();
            foreach (Deck deck in decks)
            {
                deckNames.Add(deck.id);
            }
            return deckNames;
        }
        public List<Slide> GetSlides(string deck)
        {
            Assessment assess = traitify.CreateAssesment(deck);
            assessment_id = assess.id;
            Assessment assessment = traitify.GetAssessment(assessment_id);
            List<Slide> slides = traitify.GetSlides(assessment_id);
            return slides;
        }

        public string Result(string id, List<Slide> slides)
        {
            id = assessment_id;
            traitify.SetSlideBulkUpdate(id, slides);
            AssessmentPersonalityTypes types = traitify.GetPersonalityTypes(id);
            List<AssessmentPersonalityType> personalityTypes = types.personality_types;
            return "Your type of personality is " + personalityTypes[0].personality_type.name + "! "+ personalityTypes[0].personality_type.description;
        }

    }
}

