require 'nokogiri'

class String
  def ends_with(str)
    self[length - str.length..length] == str
  end
end

def check(file1, file2)
  return true if !File.exists?(file2)
  File.mtime(file1) > File.mtime(file2)
end

def generate(file)
  filebasename = File.basename(file, ".xml")

  out = "output/" + filebasename

  code = Nokogiri::XML(open(file))

  if check(file, out + "_write.c")
    File.open(out + "_write.c", "w") do |f|
      f.puts "#include <QFile>"
      f.puts "#include <QDebug>"
      f.puts code.children.at("cinclude").text
      if not code.children.at("ccommon").nil?
        f.puts
        f.puts code.children.at("ccommon").text
        f.puts
      end
      f.puts "int main()\n{"
      f.puts "  QFile file(\"#{filebasename}.txt\");"
      f.puts "  file.open(QIODevice::WriteOnly);"
      f.puts "  QDataStream out(&file);"
      f.puts code.children.at("cwrite").text
      f.puts "\n  return 0;\n}\n";
    end
  end

  if check(file, out + "_read.c")
    File.open(out + "_read.c", "w") do |f|
      f.puts "#include <QFile>"
      f.puts "#include <QDebug>"
      f.puts "#include <assert.h>"
      f.puts code.children.at("cinclude").text
      if not code.children.at("ccommon").nil?
        f.puts
        f.puts code.children.at("ccommon").text
        f.puts
      end
      f.puts "int main()\n{"
      f.puts "  QFile file(\"#{filebasename}.txt\");"
      f.puts "  file.open(QIODevice::ReadOnly);"
      f.puts "  QDataStream in(&file);";

      f.puts code.children.at("cread").text
      f.puts "\n  return 0;\n}\n";
    end
  end

  if check(file, out + "_write.cs")
    File.open(out + "_write.cs", "w") do |f|
      f.puts "#define DEBUG"
      f.puts "using System;"
      f.puts "using System.IO;"
      f.puts "using System.Collections.Generic;"
      f.puts "using System.Diagnostics;"
      f.puts "using Qutter;"
      if not code.children.at("cscommon").nil?
        f.puts code.children.at("cscommon").text
        f.puts
      end
      f.puts "public class MainClass"
      f.puts "{";
      f.puts "  public static void Main(string[] args)"
      f.puts "  {"
      f.puts "    FileStream fs = File.OpenWrite(\"#{filebasename}.txt\");"
      f.puts code.children.at("cswrite").text
      f.puts "    fs.SetLength(fs.Position);"
      f.puts "    fs.Close();"
      f.puts "  }"
      f.puts "}";
    end
  end

  if check(file, out + "_read.cs")
    File.open(out + "_read.cs", "w") do |f|
      f.puts "#define DEBUG"
      f.puts "using System;"
      f.puts "using System.IO;"
      f.puts "using System.Collections.Generic;"
      f.puts "using System.Diagnostics;"
      f.puts "using Qutter;"
      f.puts
      if not code.children.at("cscommon").nil?
        f.puts code.children.at("cscommon").text
        f.puts
      end
      f.puts "public class MainClass"
      f.puts "{";
      f.puts "  public static void Main(string[] args)"
      f.puts "  {"
      f.puts "    FileStream fs = File.OpenRead(\"#{filebasename}.txt\");"
      f.puts "    Trace.Listeners.Add(new ConsoleTraceListener());"
      f.puts code.children.at("csread").text
      f.puts "    fs.Close();"
      f.puts "  }"
      f.puts "}";
    end
  end
end

Dir["tests/*.xml"].each do |file|
  puts "Generating tests for #{File.basename(file, ".xml")}"
  generate(file)
end

puts "Generating Makefile"
File.open("output/Makefile", "w+") do |test|
  files = [ ]
  basefiles = [ ]
  libs = "\`pkg-config --libs --cflags QtCore\`"

  Dir["tests/*.xml"].each do |file|
    filebasename = File.basename(file, ".xml")

    basefiles.push filebasename

    files.push "#{filebasename}_write.exe"
    files.push "#{filebasename}_read.exe"
    files.push "#{filebasename}_write"
    files.push "#{filebasename}_read"
  end

  test.puts "all: " + files.join(" ")
  test.puts

  basefiles.each do |filebasename|
    test.puts "#{filebasename}_write.exe: #{filebasename}_write.cs"
    test.puts "\tgmcs -r:Qutter.dll -debug #{filebasename}_write.cs"
    test.puts
    test.puts "#{filebasename}_read.exe: #{filebasename}_read.cs"
    test.puts "\tgmcs -r:Qutter.dll -debug #{filebasename}_read.cs"
    test.puts
    test.puts "#{filebasename}_write: #{filebasename}_write.c"
    test.puts "\tg++ #{filebasename}_write.c #{libs} -o #{filebasename}_write"
    test.puts
    test.puts "#{filebasename}_read: #{filebasename}_read.c"
    test.puts "\tg++ #{filebasename}_read.c #{libs} -o #{filebasename}_read"
    test.puts
  end

  test.puts "clean:"
  test.puts "\trm -rvf " + files.join(" ")
  test.puts

  basefiles.each do |fb|
    test.puts "#{fb}: #{fb}_write.exe #{fb}_read.exe #{fb}_write #{fb}_read"
    test.puts "\t@echo #{fb}"
    test.puts "\t@mono --debug #{fb}_write.exe"
    test.puts "\t@mono --debug #{fb}_read.exe"

    test.puts "\t@mono --debug #{fb}_write.exe"
    test.puts "\t@./#{fb}_read"

    test.puts "\t@./#{fb}_write"
    test.puts "\t@mono --debug #{fb}_read.exe"

    test.puts "\t@./#{fb}_write"
    test.puts "\t@./#{fb}_read"
    test.puts
  end

  test.puts "run: " + basefiles.join(" ")
end
