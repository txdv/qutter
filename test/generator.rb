require 'nokogiri'

FILENAME = "serialization.txt";

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
  f.puts
  f.puts code.children.at("ccommon").text
  f.puts
  f.puts "int main()\n{"
  f.puts "  QFile file(\"#{FILENAME}\");"
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
  f.puts
  f.puts code.children.at("ccommon").text
  f.puts
  f.puts "int main()\n{"
  f.puts "  QFile file(\"#{FILENAME}\");"
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
  f.puts "using Qutter;"
  f.puts
  f.puts code.children.at("cscommon").text
  f.puts
  f.puts "public class MainClass"
  f.puts "{";
  f.puts "  public static void Main(string[] args)"
  f.puts "  {"
  f.puts "    FileStream fs = File.OpenWrite(\"#{FILENAME}\");"
  f.puts code.children.at("cswrite").text
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
  f.puts "using Qutter;"
  f.puts "using System.Diagnostics;"
  f.puts
  f.puts code.children.at("cscommon").text
  f.puts
  f.puts "public class MainClass"
  f.puts "{";
  f.puts "  public static void Main(string[] args)"
  f.puts "  {"
  f.puts "    FileStream fs = File.OpenRead(\"#{FILENAME}\");"
  f.puts "    Trace.Listeners.Add(new ConsoleTraceListener());"
  f.puts code.children.at("csread").text
  f.puts "    fs.Close();"
  f.puts "  }"
  f.puts "}";
end
end


File.open("test", "w+") do |f|
  f.puts(filebasename + "_read")
  f.puts(filebasename + "_write")
  f.puts("mono " + filebasename + "_read.exe")
  f.puts("mono " + filebasename + "_write.exe")
end

end

Dir["tests/*.xml"].each do |file|
  puts "Generating tests for #{File.basename(file, ".xml")}"
  generate(file)
end

File.delete("test")
puts "Generating Makefile"
File.open("output/Makefile", "w+") do |test|
files = [ ]
Dir["tests/*.xml"].each do |file|
  filebasename = File.basename(file, ".xml")

  files.push "#{filebasename}_write.exe"
  files.push "#{filebasename}_read.exe"
  files.push "#{filebasename}_write"
  files.push "#{filebasename}_read"

  test.puts "#{filebasename}_write.exe: #{filebasename}_write.cs"
  test.puts "\tgmcs -r:Qutter.dll -debug #{filebasename}_write.cs"
  test.puts
  test.puts "#{filebasename}_read.exe: #{filebasename}_read.cs"
  test.puts "\tgmcs -r:Qutter.dll -debug #{filebasename}_read.cs"
  test.puts
  test.puts "#{filebasename}_write: #{filebasename}_write.c"
  test.puts "\tg++ \`pkg-config QtCore --libs --cflags\` #{filebasename}_write.c -o #{filebasename}_write"
  test.puts
  test.puts "#{filebasename}_read: #{filebasename}_read.c"
  test.puts "\tg++ \`pkg-config QtCore --libs --cflags\` #{filebasename}_read.c -o #{filebasename}_read"
  test.puts
end

  test.puts "all: " + files.join(" ")
  test.puts

  test.puts "run: all"
  files.each do |file|
    test.puts "\t@echo #{file}"
    if (file.ends_with(".exe"))
      test.puts "\t@mono --debug #{file}"
    else
      test.puts "\t@./#{file}"
    end
  end

end
