# Kindlers List

An application that reads highlights from the Kindle, translates the highlights to english and creates Anki cards out of it. Everything works without having to plug in your Kindle or running Anki with AnkiConnect installed.

## Limitations

**Hosting**

Since this program requires your Anki and Amazon passwords, I will not offer any kind of hosting. At the moment, I recommend setting this up yourself as a daemon on, for example, a Raspberry PI.

**Languages**

This program uses Deepl (although their API is not available in my country), which offers a limited number of target languages. Deepl Translator is used because it supports very common learner languages and is supposedly a very good translator.

**Two step authentication**

Amazon offers two step authentication, which must be turned off in order for this program to work. This can likely be fixed later on.
