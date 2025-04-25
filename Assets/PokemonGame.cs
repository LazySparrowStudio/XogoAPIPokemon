using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class Pokemon
{
    public string name;
    public int weight;
    public Sprites sprites;
}

[System.Serializable]
public class Sprites
{
    public string front_default;
}

public class PokemonGame : MonoBehaviour
{
    int roundsPlayed = 0;
    int maxRounds = 10;
    public Button[] buttons;
    public RawImage[] pokemonImages;
    public Text feedbackText;
    public GameObject restartButton;
    public GameObject quitButton;

    private Pokemon[] loadedPokemon = new Pokemon[3];
    private int correctIndex;
    private string[] pokemonList = new string[]
    {
        "pikachu", "bulbasaur", "charmander", "squirtle",
        "jigglypuff", "snorlax", "eevee", "machop",
        "psyduck", "gengar", "dragonite", "mewtwo"
    };

    void Start()
    {
        StartCoroutine(LoadPokemonGame());
    }

    IEnumerator LoadPokemonGame()
    {
        feedbackText.text = "Cargando Pokémon...";
        List<string> selected = new List<string>();

        // Evitar duplicados
        while (selected.Count < 3)
        {
            string candidate = pokemonList[Random.Range(0, pokemonList.Length)];
            if (!selected.Contains(candidate))
                selected.Add(candidate);
        }

        for (int i = 0; i < 3; i++)
        {
            yield return StartCoroutine(LoadPokemon(selected[i], i));
        }

        SetupQuestion();

    }

    IEnumerator LoadPokemon(string name, int index)
    {
        string url = $"https://pokeapi.co/api/v2/pokemon/{name}";
        UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result == UnityWebRequest.Result.Success)
        {
            loadedPokemon[index] = JsonUtility.FromJson<Pokemon>(req.downloadHandler.text);

            string imageUrl = loadedPokemon[index].sprites.front_default;
            UnityWebRequest texReq = UnityWebRequestTexture.GetTexture(imageUrl);
            yield return texReq.SendWebRequest();

            if (texReq.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(texReq);
                pokemonImages[index].texture = texture;
                pokemonImages[index].color = Color.white;

            }
        }


        else
        {
            Debug.LogError("Error al cargar: " + name);
            loadedPokemon[index] = null;
        }
    }

    void SetupQuestion()
    {
        // Calcular cuál es el más pesado
        int maxWeight = -1;
        correctIndex = 0;
        for (int i = 0; i < loadedPokemon.Length; i++)
        {

            buttons[i].GetComponentInChildren<Text>().text = loadedPokemon[i].name;

            if (loadedPokemon[i].weight > maxWeight)
            {
                maxWeight = loadedPokemon[i].weight;
                correctIndex = i;
            }

            int capturedIndex = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnPokemonSelected(capturedIndex));
            buttons[i].interactable = true;
        }

        feedbackText.text = "¿Cuál de estos Pokémon pesa más?";
    }

    void OnPokemonSelected(int index)
    {
        if (index == correctIndex)
        {
            feedbackText.text = "¡Correcto! 🎉";
        }
        else
        {
            feedbackText.text = $"Incorrecto. Era {loadedPokemon[correctIndex].name}";
        }

        // Opcional: desactivar botones después de responder
        foreach (Button b in buttons)
            b.interactable = false;


        roundsPlayed++;

        if (roundsPlayed < maxRounds)
        {
            // Espera un poco y vuelve a cargar otra ronda
            StartCoroutine(NextRoundAfterDelay(2f));
        }
        else
        {
            // Juego terminado
            feedbackText.text = "¡Juego terminado! ¿Jugar de nuevo o salir?";
            restartButton.SetActive(true);
            quitButton.SetActive(true);
        }
    }

    IEnumerator NextRoundAfterDelay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        StartCoroutine(LoadPokemonGame());
    }

    public void RestartGame()
    {
        roundsPlayed = 0;
        restartButton.SetActive(false);
        quitButton.SetActive(false);
        StartCoroutine(LoadPokemonGame());
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }
}



