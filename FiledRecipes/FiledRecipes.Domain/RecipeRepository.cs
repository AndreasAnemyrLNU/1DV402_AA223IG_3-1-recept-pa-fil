using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FiledRecipes.Domain
{
    /// <summary>
    /// Holder for recipes.
    /// </summary>
    public class RecipeRepository : IRecipeRepository
    {
        /// <summary>
        /// Represents the recipe section.
        /// </summary>
        private const string SectionRecipe = "[Recept]";

        /// <summary>
        /// Represents the ingredients section.
        /// </summary>
        private const string SectionIngredients = "[Ingredienser]";

        /// <summary>
        /// Represents the instructions section.
        /// </summary>
        private const string SectionInstructions = "[Instruktioner]";

        /// <summary>
        /// Occurs after changes to the underlying collection of recipes.
        /// </summary>
        public event EventHandler RecipesChangedEvent;

        /// <summary>
        /// Specifies how the next line read from the file will be interpreted.
        /// </summary>
        private enum RecipeReadStatus { Indefinite, New, Ingredient, Instruction };

        /// <summary>
        /// Collection of recipes.
        /// </summary>
        private List<IRecipe> _recipes;

        /// <summary>
        /// The fully qualified path and name of the file with recipes.
        /// </summary>
        private string _path;

        /// <summary>
        /// Indicates whether the collection of recipes has been modified since it was last saved.
        /// </summary>
        public bool IsModified { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the RecipeRepository class.
        /// </summary>
        /// <param name="path">The path and name of the file with recipes.</param>
        public RecipeRepository(string path)
        {
            // Throws an exception if the path is invalid.
            _path = Path.GetFullPath(path);

            _recipes = new List<IRecipe>();
        }

        /// <summary>
        /// Returns a collection of recipes.
        /// </summary>
        /// <returns>A IEnumerable&lt;Recipe&gt; containing all the recipes.</returns>
        public virtual IEnumerable<IRecipe> GetAll()
        {
            // Deep copy the objects to avoid privacy leaks.
            return _recipes.Select(r => (IRecipe)r.Clone());
        }

        /// <summary>
        /// Returns a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to get.</param>
        /// <returns>The recipe at the specified index.</returns>
        public virtual IRecipe GetAt(int index)
        {
            // Deep copy the object to avoid privacy leak.
            return (IRecipe)_recipes[index].Clone();
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="recipe">The recipe to delete. The value can be null.</param>
        public virtual void Delete(IRecipe recipe)
        {
            // If it's a copy of a recipe...
            if (!_recipes.Contains(recipe))
            {
                // ...try to find the original!
                recipe = _recipes.Find(r => r.Equals(recipe));
            }
            _recipes.Remove(recipe);
            IsModified = true;
            OnRecipesChanged(EventArgs.Empty);
        }

        /// <summary>
        /// Deletes a recipe.
        /// </summary>
        /// <param name="index">The zero-based index of the recipe to delete.</param>
        public virtual void Delete(int index)
        {
            Delete(_recipes[index]);
        }

        /// <summary>
        /// Raises the RecipesChanged event.
        /// </summary>
        /// <param name="e">The EventArgs that contains the event data.</param>
        protected virtual void OnRecipesChanged(EventArgs e)
        {
            // Make a temporary copy of the event to avoid possibility of 
            // a race condition if the last subscriber unsubscribes 
            // immediately after the null check and before the event is raised.
            EventHandler handler = RecipesChangedEvent;

            // Event will be null if there are no subscribers. 
            if (handler != null)
            {
                // Use the () operator to raise the event.
                handler(this, e);
            }
        }

        /// <summary>
        /// AA 141020
        /// </summary>
        public void Load()
        {
            //Påbörjar att försöka först hur loaden ska fungera
            //Metod ska returnea en list med objekt

            RecipeReadStatus recipeReadStatus = new RecipeReadStatus();
            string line;
            Recipe currentRecipe = null;
            
            //1
            List<IRecipe> recipes = new List<IRecipe>();
            
            
            //2
            using (StreamReader reader = new StreamReader(_path))
            {
                //3
                while ((line = reader.ReadLine()) != null) //WHILE "READING FILE" START
                {
                    switch (line) //SWITCH "SECTION" START
                    {
                    //a
                        case "":
                            continue;
                    //b
                        case SectionRecipe:
                            recipeReadStatus = RecipeReadStatus.New;
                            continue;;
                    //c
                        case SectionIngredients:
                            recipeReadStatus = RecipeReadStatus.Ingredient;
                            continue;
                    //d
                        case SectionInstructions:
                            recipeReadStatus = RecipeReadStatus.Instruction;
                            continue;
                    }// SWITCH "SECTION" END
                    
                    //e
                    switch (recipeReadStatus) //SWITCH "RECIPEREADSTATUS" START
                    {
                        //i
                        case RecipeReadStatus.New:
                            // 1
                            currentRecipe = new Recipe(line);
                            recipes.Add(currentRecipe);
                            break;
                        //ii
                        case RecipeReadStatus.Ingredient:
                            //1 
                            // Split line of ingredient separated by semicolon...
                            string[] values = line.Split(';');
                            //2
                            if (values.Length != 3)
                            {
                                throw new FileFormatException();
                            }
                            // Throws an Exception in ingrediient has no Name in field...
                            if (values[2] == "")
                            {
                                throw new Exception("Namn på ingrediens är obligatorisk!");
                            }
                            //3
                            Ingredient ingredientObj = new Ingredient();
                            ingredientObj.Amount = values[0];
                            ingredientObj.Measure = values[1];
                            ingredientObj.Name = values[2];
                            //4
                            currentRecipe.Add(ingredientObj);
                            break;
                        //iii
                        case RecipeReadStatus.Instruction:
                            currentRecipe.Add(line);
                            break;
                        //vi
                        default:
                            throw new FileFormatException();
                    } //SWITCH "RECIPEREADSTATUS" END             

                } //WHILE "READING FILE" END

            }// USING END

            // 4 Sort list. CompareTo reciept can sort by recipe.Name, CompareTo(Recipe other)
            recipes.Sort();
            
            // 5 Tilldela recipes -> _recipes
            _recipes = recipes;

            // 6
            IsModified = false;
 
            // 7
            OnRecipesChanged(EventArgs.Empty);
            

        }// LOAD METHOD END

       
        /// <summary>
        /// AA 141020
        /// </summary>
        public void Save()
        {

            using (StreamWriter writer = new StreamWriter(_path))
            {
                foreach (Recipe recipe in _recipes)
                {
                    // [Recept]
                    writer.WriteLine(SectionRecipe);
                    writer.WriteLine(recipe.Name);
                    // [Ingredient]
                    writer.WriteLine(SectionIngredients);
                    foreach (Ingredient ingredient in recipe.Ingredients)
                    {
                        writer.WriteLine(string.Format("{0};{1};{2}",
                            ingredient.Amount, ingredient.Measure, ingredient.Name));
                    }
                    // [Instruction]
                    writer.WriteLine(SectionInstructions);
                    foreach (string instruction in recipe.Instructions)
                    {
                        writer.WriteLine(instruction);
                    }
                }
            }
            IsModified = false;
            OnRecipesChanged(EventArgs.Empty);
        }

    }
}
