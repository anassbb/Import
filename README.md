# Kata C# - Import Data Files

Moteur d'import générique qui lit les mêmes données métier depuis plusieurs formats de fichiers
et les convertit en une représentation JSON commune.

## Énoncé

| Champ            | Position CSV | Largeur fixe | Type                          |
|------------------|--------------|--------------|-------------------------------|
| OriginalSourceId | 1            | 1-4          | string                        |
| TransactionDate  | 2            | 5-12         | date (`yyyyMMdd` en largeur fixe, `yyyy-MM-dd` en CSV) |
| Value            | 3            | 13-20        | decimal (2 décimales implicites) |
| Rate             | 4            | 21-25        | decimal (3 décimales implicites) |

Entrée CSV :

```
F001,2026-10-10,5050.89,0.145
F003,2026-10-20,12200.34,1.250
```

Entrée largeur fixe :

```
F001202610100050508900145
F003202610200122003401250
```

Sortie attendue (JSON compact), identique pour les deux formats :

```json
[{"OriginalSourceId":"F001","TransactionDate":"2026-10-10","Value":5050.89,"Rate":0.145},{"OriginalSourceId":"F003","TransactionDate":"2026-10-20","Value":12200.34,"Rate":1.25}]
```

## Prérequis

- .NET SDK 10

## Lancer les tests

```bash
dotnet test
```

## Utiliser la CLI

```bash
dotnet run --project src/Import.Cli -- samples/transactions.csv
dotnet run --project src/Import.Cli -- samples/transactions.txt
```

Le format d'entrée est détecté à partir de l'extension (`.csv` pour le CSV, `.txt` / `.dat` / `.fwf` pour la largeur fixe).
Il peut aussi être forcé :

```bash
dotnet run --project src/Import.Cli -- mon_fichier.dat --format fixed --output json
```

Codes de sortie : `0` succès, `1` mauvais arguments, `2` erreur de données ou de fichier.
Le JSON est écrit sur la sortie standard, les erreurs sur la sortie d'erreur, par exemple :

```
Line 2, field 'TransactionDate': invalid value 'bad'. ...
```

## Structure

```
src/
  Import.Core/            moteur d'import (aucune dépendance externe)
    ImportRecord          ligne importée, indépendante du format : nom du champ -> valeur typée
    ImportPipeline        relie un lecteur à un écrivain
    Values/               conversion texte -> valeur (texte, date, décimal, décimales implicites)
    Readers/              lecteurs : base commune ligne par ligne, CSV, largeur fixe
    Writers/              écrivain JSON
    Registry/             catalogue des formats (nom / extension -> fabrique)
    Transactions/         configuration du kata : les layouts CSV et largeur fixe
  Import.Cli/             application console
tests/
  Import.Tests/           tests unitaires et d'acceptation (xUnit)
samples/                  fichiers d'exemple de l'énoncé
```

## Choix de conception

**Un format pivot.** Chaque lecteur produit des `ImportRecord`, chaque écrivain les consomme.
N'importe quel format d'entrée peut donc être combiné avec n'importe quel format de sortie.

**Le moteur est séparé de la configuration.** Les positions des champs sont décrites dans
`Transactions/TransactionImport.cs`, recopiées du tableau de l'énoncé. Le reste du code ne connaît pas
les transactions et peut servir à importer d'autres données.

**Ajouter sans modifier (principe ouvert/fermé).**

| Évolution                          | Ce qu'il faut faire                                          |
|------------------------------------|--------------------------------------------------------------|
| Nouveau champ                      | Une ligne de mapping dans le layout                          |
| Nouveau type de valeur             | Une classe qui implémente `IValueParser`                     |
| Nouveau format d'entrée            | Une classe `IRecordReader` + `Register(...)` dans le registre |
| Nouveau format de sortie           | Une classe `IRecordWriter` + `Register(...)` dans le registre |
| CSV avec `;`, en-tête, colonnes ignorées | Options de `CsvLayout`                                 |

Les tests de `ExtensibilityTests` le vérifient : un type Oui/Non, un champ supplémentaire,
un format d'entrée `clé=valeur` et un format de sortie texte sont ajoutés sans toucher à `Import.Core`.

**Une seule boucle pour les formats ligne par ligne.** `LineRecordReader` gère la lecture, les lignes vides,
l'en-tête, la numérotation des lignes et les erreurs. Les lecteurs CSV et largeur fixe n'implémentent
que le découpage d'une ligne (`Split`).

**Gros fichiers.** La lecture est paresseuse (`yield return`) et le JSON est écrit par blocs de 64 Ko :
la mémoire utilisée ne dépend pas de la taille du fichier.

**Erreurs explicites.** Une valeur invalide lève une `ImportException` avec le numéro de ligne et le nom
du champ. Les layouts sont validés à leur création (doublons, colonnes en double, plages qui se chevauchent),
une mauvaise configuration est donc détectée au démarrage et non en cours d'import.

## Points d'attention

- **Montants en `decimal`**, jamais en `double`, pour ne perdre aucune précision.
- **Culture invariante** pour les dates et les nombres : le résultat est le même sur un poste en français
  (où le séparateur décimal est la virgule) que sur un serveur en anglais.
- **`1.250` devient `1.25`** : un `decimal` garde son nombre de décimales, le JSON écrirait `1.250`.
  L'écrivain JSON normalise la valeur pour obtenir la sortie attendue.
- **Nombre de colonnes CSV strict** : une virgule utilisée comme séparateur décimal (`50,50`) ajouterait
  une colonne et décalerait les valeurs sans erreur. La ligne est rejetée.
- **Largeur fixe tolérante en fin de ligne** : les caractères au-delà du dernier champ sont ignorés.

## Limites connues

- Le CSV ne gère pas les champs entre guillemets (`"Dupont, Jean"`), non demandé par l'énoncé.
  Seule la méthode `CsvRecordReader.Split` serait à adapter.
- Un seul format de sortie (JSON) est fourni.
