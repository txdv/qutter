Qutter
======

Description
-----------

The goal of Qutter is to provide serialization and deserialization
of Qt and .NET objects. Objects from Qt are mapped to .NET objects
like QString to string, QMap to Dictionary and so on. This library
is usefull if you want have to communuicate with Qt applications
which send serialized objects over the network and you dont want
to have Qt installed or if you can't install it, a valid scenario
would be a quassel client (distributed irc application, the
client part) for WP7, since the WP7 is locked and only C# code is
allowed to be run.

Supported Qt objects.
---------------------
The following provides a list of qt object serializers which are
provided with this library.

> Qt Object        - .NET Object
> -----------------------------------------------
>  1. bool         - bool
>  2. int          - int
>  3. uint         - uint
>  4. QChar        - char
>  5. QVariantMap  - Dictionary<string, QVariant>
>  6. QVariantList - List<QVariant>
>  7. QString      - string
>  8. QStringList  - List<string>
>  9. QByteArray   - byte[]
> 10. QDate        - DateTime
> 
> 11. QVariant     - QVariant
> 12. QMap<T1, T2> - Dictionary<T1, T2>
> 13. QList<T1>    - List<T1>


.NET Runtime
------------

A .NET 3.5 compatible runtime is required.
Mono works fine too.

License
-------

Qutter is licensed under the terms of the MIT X11 license.

Authors
-------

* [Andrius Bentkus](mailto: andrius.bentkus@gmail.com)

Contacts
--------

You can reach Andrius Bentkus via email or IRC, mostly txdv or bentkus
on every major irc network.
