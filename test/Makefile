
.PHONY: all run clean

all:
	ln -fs ../../bin/Debug/Qutter.dll output/
	mkdir -p output
	ruby generator.rb
	cd output; $(MAKE) $(MFLAGS) all

run: all
	cd output; $(MAKE) $(MFLAGS) run

clean:
	rm -f output/*
