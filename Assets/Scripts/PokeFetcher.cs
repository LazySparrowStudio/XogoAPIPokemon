using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class PokeFetcher : MonoBehaviour
{
    public string[] pokemonNames = { "pikachu", "bulbasaur", "charmander", "squirtle", "jigglypuff", "snorlax", "eevee" };

    void Start()
    {
        StartCoroutine(GetRandomPokemon());
    }

    IEnumerator GetRandomPokemon()
    {
        string name = pokemonNames[Random.Range(0, pokemonNames.Length)];
        string url = $"https://pokeapi.co/api/v2/pokemon/{name.ToLower()}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            string json = req.downloadHandler.text;
            Pokemon pokemon = JsonUtility.FromJson<Pokemon>(json);
            Debug.Log($"{pokemon.name} pesa {pokemon.weight}");
        }
        else
        {
            Debug.LogError("Error al obtener Pokémon: " + req.error);
        }
    }
}
