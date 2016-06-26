
using com.traitify.net.TraitifyLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraitifyAPI
{
    class Personality
    {
        
        static void Main(string[] args)
        {
            Traitify traitify = new Traitify("https://api.traitify.com", "bt5bo444mk38efl4crbat39scv", "pl17doqlrachpa7jg5oqe3ja7p", "v1");

            List<Deck> decks = traitify.GetDecks();
            foreach (Deck deck in decks)
            {
                Console.WriteLine(deck.id);
            }
            Console.WriteLine("Please, choose one test");
            string testName = Console.ReadLine();

            Assessment assess = traitify.CreateAssesment(testName);
            string assessment_id = assess.id;
            Assessment assessment = traitify.GetAssessment(assessment_id);
            List<Slide> slides = traitify.GetSlides(assessment_id);

            Console.WriteLine("Please give the asnswer true or false to the following statements");
            foreach (Slide slide in slides)
            {
                Console.WriteLine(slide.caption);
                slide.time_taken = 600;
                slide.response = Convert.ToBoolean(Console.ReadLine());                          
            }

            traitify.SetSlideBulkUpdate(assessment_id, slides);
            AssessmentPersonalityTypes types = traitify.GetPersonalityTypes(assessment_id);
            List<AssessmentPersonalityType> personalityTypes = types.personality_types;
            Console.WriteLine("Your type of personality is: " + personalityTypes[0].personality_type.name);
            Console.WriteLine(personalityTypes[0].personality_type.description);
            Console.WriteLine(personalityTypes[0].personality_type.badge.image_medium);
        }
    }
}
