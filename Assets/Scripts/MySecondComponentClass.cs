/*using UnityEngine;

public class MySecondComponentClass : MonoBehaviour {
    static MySecondComponentClass instance;
    int nombre = 2;
    char caractere = 'e';
    double nombreAVirgule = 5.34923402459;
    string chaineDeCaractere = "I like trains";
    bool suisJeCon = true;

    int truc = 5;
    string CommeJeVeux = "untrucaupif";

	// Use this for initialization
	void Start() {
        instance = this;
        Test();
	}
	
	// Update is called once per frame
	void Update() {
        //int machin = Addition(truc, truc + 2);
	}

    int Addition(int nomDeLaVariable, int nomDeLaVariable2) {
        int resultat = nomDeLaVariable + nomDeLaVariable2;
        return resultat;
    }

    public static void Test() {
        print(instance.CommeJeVeux);
        string OuPas = "OuPas";
        string Resultat = instance.CommeJeVeux + OuPas;
        instance.CommeJeVeux = Resultat;
        print(instance.CommeJeVeux);
    }
}

public class Caca {
    public static void Fonction() { }
}*/