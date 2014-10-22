﻿using FiledRecipes.Domain;
using FiledRecipes.App.Mvp;
using FiledRecipes.Properties;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FiledRecipes.Views
{
    /// <summary>
    /// 
    /// </summary>
    public class RecipeView : ViewBase, IRecipeView
    {
        /// <summary>
        /// AA 141020 impl
        /// </summary>
        /// <param name="recipe"></param>
        public void Show(IRecipe recipe)
        {
            Console.Clear();
            Header = recipe.Name;
            ShowHeaderPanel();
            Console.WriteLine();
            Console.WriteLine("Ingredienser");
            Console.WriteLine("============");
            foreach (Ingredient ingredient in recipe.Ingredients)
            {
                Console.WriteLine(ingredient);
            }
            Console.WriteLine();
            Console.WriteLine("Görs så här");
            Console.WriteLine("===========");



            recipe.Instructions
            int i = 1;           
            foreach (string instruction in recipe.Instructions)
            {          
                Console.WriteLine(string.Format("({0})", i));
                if (instruction == "")
                {

                    Console.WriteLine(instruction);
                    i++;
                }

            }


        }
        /// <summary>
        /// AA 141020 impl
        /// </summary>
        /// <param name="recipes"></param>
        public void Show(IEnumerable<IRecipe> recipes)
        {
            throw new NotImplementedException();
        }
    }
}
